using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using OneView.VirtualScanner.Core.Models;
using OneView.VirtualScanner.Core.Services;
using OneView.VirtualScanner.Source.Capabilities;
using OneView.VirtualScanner.Source.Twain;

namespace OneView.VirtualScanner.Source;

/// <summary>
/// TWAIN Data Source entry point. Exported as a native function so the TWAIN DSM can load it.
/// State machine: Closed → Opened → Enabled → Transferring.
/// </summary>
public static class TwainSource
{
    // ── State ────────────────────────────────────────────────────────────────
    private static readonly ProfileManager ProfileMgr = new();
    private static readonly ScanLogger Logger = new();
    private static readonly PageQueue Queue = new();

    private static TW_STATUS _status = new() { ConditionCode = TWCC.SUCCESS };
    private static int _state = 3; // 3=closed(ds not opened), 4=opened, 5=enabled, 6=transferring
    private static string[]? _pendingPages;
    private static int _pageIndex;
    private static ActiveConfig? _active;
    private static ScanProfile? _profile;
    private static int _newCursor;
    private static BehaviorSettings _behavior = new();

    // ── DS_Entry ─────────────────────────────────────────────────────────────
    [UnmanagedCallersOnly(EntryPoint = "DS_Entry",
        CallConvs = new[] { typeof(CallConvStdcall) })]
    public static unsafe ushort DS_Entry(
        TW_IDENTITY* pOrigin,
        uint dg,
        ushort dat,
        ushort msg,
        void* pData)
    {
        try
        {
            return Dispatch(pOrigin, dg, dat, msg, pData);
        }
        catch (Exception ex)
        {
            Logger.Error($"DS_Entry exception: {ex.Message}");
            _status.ConditionCode = TWCC.OPERATIONERROR;
            return TWRC.FAILURE;
        }
    }

    private static unsafe ushort Dispatch(
        TW_IDENTITY* pOrigin, uint dg, ushort dat, ushort msg, void* pData)
    {
        Logger.Info($"DS_Entry DG=0x{dg:X4} DAT=0x{dat:X4} MSG=0x{msg:X4} state={_state}");

        return (dg, dat, msg) switch
        {
            (DG.CONTROL, DAT.IDENTITY, MSG.OPENDS)      => HandleOpenDS(pData),
            (DG.CONTROL, DAT.IDENTITY, MSG.CLOSEDS)     => HandleCloseDS(),
            (DG.CONTROL, DAT.STATUS, MSG.GET)           => HandleGetStatus(pData),
            (DG.CONTROL, DAT.CAPABILITY, MSG.GET)       => HandleCapGet(pData),
            (DG.CONTROL, DAT.CAPABILITY, MSG.GETCURRENT) => HandleCapGet(pData),
            (DG.CONTROL, DAT.CAPABILITY, MSG.GETDEFAULT) => HandleCapGet(pData),
            (DG.CONTROL, DAT.CAPABILITY, MSG.SET)       => HandleCapSet(pData),
            (DG.CONTROL, DAT.CAPABILITY, MSG.RESET)     => HandleCapReset(pData),
            (DG.CONTROL, DAT.USERINTERFACE, MSG.ENABLEDS) => HandleEnableDS(pData),
            (DG.CONTROL, DAT.USERINTERFACE, MSG.DISABLEDS) => HandleDisableDS(),
            (DG.IMAGE,   DAT.IMAGEINFO, MSG.GET)         => HandleImageInfo(pData),
            (DG.CONTROL, DAT.SETUPMEMXFER, MSG.GET)      => HandleSetupMemXfer(pData),
            (DG.IMAGE,   DAT.IMAGEMEMXFER, MSG.GET)      => HandleImageMemXfer(pData),
            (DG.IMAGE,   DAT.IMAGENATIVEXFER, MSG.GET)   => HandleImageNativeXfer(pData),
            (DG.CONTROL, DAT.PENDINGXFERS, MSG.ENDXFER)  => HandleEndXfer(pData),
            (DG.CONTROL, DAT.PENDINGXFERS, MSG.STOPFEEDER) => HandleStopFeeder(pData),
            _ => Unsupported(dg, dat, msg)
        };
    }

    // ── Identity ─────────────────────────────────────────────────────────────
    private static unsafe ushort HandleOpenDS(void* pData)
    {
        if (_state != 3)
        {
            _status.ConditionCode = TWCC.SEQERROR;
            return TWRC.FAILURE;
        }
        _behavior = ProfileMgr.LoadBehavior();
        _active = ProfileMgr.LoadActiveConfig();
        if (!string.IsNullOrEmpty(_active.ActiveProfile))
        {
            var all = ProfileMgr.LoadAll();
            _profile = all.FirstOrDefault(p => p.Name == _active.ActiveProfile);
        }

        var id = (TW_IDENTITY*)pData;
        id->Manufacturer   = _behavior.Manufacturer[..Math.Min(_behavior.Manufacturer.Length, 33)];
        id->ProductFamily  = _behavior.ProductFamily[..Math.Min(_behavior.ProductFamily.Length, 33)];
        id->ProductName    = _behavior.SourceName[..Math.Min(_behavior.SourceName.Length, 33)];
        id->ProtocolMajor  = 2;
        id->ProtocolMinor  = 4;
        id->SupportedGroups = DG.CONTROL | DG.IMAGE;
        id->Version.MajorNum = 1;
        id->Version.MinorNum = 0;
        id->Version.Info = "1.0";

        _state = 4;
        Logger.Info($"OpenDS: profile={_profile?.Name ?? "(none)"}");
        _status.ConditionCode = TWCC.SUCCESS;
        return TWRC.SUCCESS;
    }

    private static ushort HandleCloseDS()
    {
        _state = 3;
        _pendingPages = null;
        Logger.Info("CloseDS");
        _status.ConditionCode = TWCC.SUCCESS;
        return TWRC.SUCCESS;
    }

    private static unsafe ushort HandleGetStatus(void* pData)
    {
        var s = (TW_STATUS*)pData;
        *s = _status;
        _status.ConditionCode = TWCC.SUCCESS; // clear after read
        return TWRC.SUCCESS;
    }

    // ── Capabilities ─────────────────────────────────────────────────────────
    private static unsafe ushort HandleCapGet(void* pData)
    {
        var cap = (TW_CAPABILITY*)pData;
        _status.ConditionCode = TWCC.SUCCESS;

        switch (cap->Cap)
        {
            case CAP.XFERCOUNT:
                cap->ConType = TWON.ONEVALUE;
                cap->hContainer = CapabilityHelper.AllocOneValue(TWTY.INT16, unchecked((uint)(short)-1));
                return TWRC.SUCCESS;

            case CAP.ICAP_XFERMECH:
                cap->ConType = TWON.ENUMERATION;
                cap->hContainer = CapabilityHelper.AllocEnumeration(TWTY.UINT16,
                    new ushort[] { TWSX.NATIVE, TWSX.MEMORY }, 0, 0);
                return TWRC.SUCCESS;

            case CAP.ICAP_PIXELTYPE:
                var pt = _profile?.PixelType == PixelType.Grayscale ? TWPT.GRAY
                       : _profile?.PixelType == PixelType.BlackAndWhite ? TWPT.BW
                       : TWPT.RGB;
                cap->ConType = TWON.ONEVALUE;
                cap->hContainer = CapabilityHelper.AllocOneValue(TWTY.UINT16, pt);
                return TWRC.SUCCESS;

            case CAP.ICAP_XRESOLUTION:
            case CAP.ICAP_YRESOLUTION:
                var dpi = _profile?.Dpi ?? 300;
                var fx = TW_FIX32.FromDouble(dpi);
                uint fxU = unchecked((uint)((fx.Frac << 16) | (ushort)fx.Whole));
                cap->ConType = TWON.ONEVALUE;
                cap->hContainer = CapabilityHelper.AllocOneValue(TWTY.FIX32, fxU);
                return TWRC.SUCCESS;

            case CAP.FEEDERENABLED:
                cap->ConType = TWON.ONEVALUE;
                cap->hContainer = CapabilityHelper.AllocOneValue(TWTY.BOOL,
                    _profile?.FeederMode == FeederMode.ADF ? 1u : 0u);
                return TWRC.SUCCESS;

            case CAP.FEEDERLOADED:
                cap->ConType = TWON.ONEVALUE;
                cap->hContainer = CapabilityHelper.AllocOneValue(TWTY.BOOL,
                    _behavior.FeederAlwaysLoaded ? 1u : 0u);
                return TWRC.SUCCESS;

            case CAP.DUPLEXENABLED:
            case CAP.DUPLEX:
                cap->ConType = TWON.ONEVALUE;
                cap->hContainer = CapabilityHelper.AllocOneValue(TWTY.BOOL,
                    _profile?.Duplex == true ? 1u : 0u);
                return TWRC.SUCCESS;

            case CAP.UICONTROLLABLE:
                cap->ConType = TWON.ONEVALUE;
                cap->hContainer = CapabilityHelper.AllocOneValue(TWTY.BOOL, 1);
                return TWRC.SUCCESS;

            case CAP.ICAP_UNITS:
                cap->ConType = TWON.ONEVALUE;
                cap->hContainer = CapabilityHelper.AllocOneValue(TWTY.UINT16, 0); // TWUN_INCHES
                return TWRC.SUCCESS;

            case CAP.ICAP_COMPRESSION:
                cap->ConType = TWON.ONEVALUE;
                cap->hContainer = CapabilityHelper.AllocOneValue(TWTY.UINT16, 0); // TWCP_NONE
                return TWRC.SUCCESS;

            case CAP.CAP_SUPPORTEDCAPS:
                cap->ConType = TWON.ONEVALUE;
                cap->hContainer = CapabilityHelper.AllocOneValue(TWTY.UINT16, 0);
                return TWRC.SUCCESS;

            default:
                _status.ConditionCode = TWCC.CAPUNSUPPORTED;
                return TWRC.FAILURE;
        }
    }

    private static unsafe ushort HandleCapSet(void* pData)
    {
        // Accept but mostly ignore for MVP
        var cap = (TW_CAPABILITY*)pData;
        Logger.Info($"CapSet: cap=0x{cap->Cap:X4}");
        _status.ConditionCode = TWCC.SUCCESS;
        return TWRC.SUCCESS;
    }

    private static unsafe ushort HandleCapReset(void* pData)
    {
        _status.ConditionCode = TWCC.SUCCESS;
        return TWRC.SUCCESS;
    }

    // ── Enable / Disable ─────────────────────────────────────────────────────
    private static unsafe ushort HandleEnableDS(void* pData)
    {
        if (_state != 4)
        {
            _status.ConditionCode = TWCC.SEQERROR;
            return TWRC.FAILURE;
        }
        if (_profile == null)
        {
            Logger.Warn("EnableDS: no active profile");
            _status.ConditionCode = TWCC.OPERATIONERROR;
            return TWRC.FAILURE;
        }

        try
        {
            _active = ProfileMgr.LoadActiveConfig();
            var (pages, newCursor) = Queue.PrepareNextScan(_profile, _active!.Cursor);
            _pendingPages = pages;
            _pageIndex = 0;
            _newCursor = newCursor;
        }
        catch (Exception ex)
        {
            Logger.Error($"EnableDS: render failed: {ex.Message}");
            _status.ConditionCode = TWCC.OPERATIONERROR;
            return TWRC.FAILURE;
        }

        _state = 5;
        Logger.Info($"EnableDS: {_pendingPages?.Length ?? 0} pages queued");

        // Signal to the app that data is ready (post MSG_XFERREADY)
        // In a real TWAIN DS we'd post a message back; for simplicity we return XFERDONE
        // The app will proceed to request image info.
        _status.ConditionCode = TWCC.SUCCESS;
        return TWRC.SUCCESS;
    }

    private static ushort HandleDisableDS()
    {
        _state = 4;
        _pendingPages = null;
        _status.ConditionCode = TWCC.SUCCESS;
        return TWRC.SUCCESS;
    }

    // ── Image Transfer ────────────────────────────────────────────────────────
    private static unsafe ushort HandleImageInfo(void* pData)
    {
        if (_pendingPages == null || _pageIndex >= _pendingPages.Length)
        {
            _status.ConditionCode = TWCC.SEQERROR;
            return TWRC.FAILURE;
        }

        var imgInfo = (TW_IMAGEINFO*)pData;
        var path = _pendingPages[_pageIndex];

        using var bmp = new System.Drawing.Bitmap(path);
        var dpi = _profile?.Dpi ?? 300;
        imgInfo->XResolution = TW_FIX32.FromDouble(dpi);
        imgInfo->YResolution = TW_FIX32.FromDouble(dpi);
        imgInfo->ImageWidth  = bmp.Width;
        imgInfo->ImageLength = bmp.Height;
        imgInfo->SamplesPerPixel = 3;
        imgInfo->BitsPerSample = new short[] { 8, 8, 8, 0, 0, 0, 0, 0 };
        imgInfo->BitsPerPixel = 24;
        imgInfo->Planar = 0;
        imgInfo->PixelType = (short)TWPT.RGB;
        imgInfo->Compression = 0; // TWCP_NONE

        _status.ConditionCode = TWCC.SUCCESS;
        return TWRC.SUCCESS;
    }

    private static unsafe ushort HandleSetupMemXfer(void* pData)
    {
        var setup = (TW_SETUPMEMXFER*)pData;
        setup->MinBufSize = 65536;
        setup->MaxBufSize = 1024 * 1024;
        setup->Preferred  = 65536;
        _status.ConditionCode = TWCC.SUCCESS;
        return TWRC.SUCCESS;
    }

    private static unsafe ushort HandleImageMemXfer(void* pData)
    {
        // Full memory transfer implementation - sends bitmap rows
        if (_pendingPages == null || _pageIndex >= _pendingPages.Length)
        {
            _status.ConditionCode = TWCC.SEQERROR;
            return TWRC.FAILURE;
        }

        var xfer = (TW_IMAGEMEMXFER*)pData;
        var path = _pendingPages[_pageIndex];
        using var bmp = new System.Drawing.Bitmap(path);

        // Load full image data into provided buffer
        var rgbData = BitmapToRgb24(bmp);
        int bytesPerRow = bmp.Width * 3;
        int rows = bmp.Height;

        xfer->Compression = 0;
        xfer->BytesPerRow = (uint)bytesPerRow;
        xfer->Columns = (uint)bmp.Width;
        xfer->Rows = (uint)rows;
        xfer->XOffset = 0;
        xfer->YOffset = 0;
        xfer->BytesWritten = (uint)rgbData.Length;

        // Copy into caller-provided memory
        if (xfer->Memory_TheMem != IntPtr.Zero && xfer->Memory_Length >= rgbData.Length)
            Marshal.Copy(rgbData, 0, xfer->Memory_TheMem, rgbData.Length);

        _status.ConditionCode = TWCC.SUCCESS;
        return TWRC.XFERDONE;
    }

    private static unsafe ushort HandleImageNativeXfer(void* pData)
    {
        // Native transfer: returns a DIB handle in *pData (IntPtr)
        if (_pendingPages == null || _pageIndex >= _pendingPages.Length)
        {
            _status.ConditionCode = TWCC.SEQERROR;
            return TWRC.FAILURE;
        }

        var path = _pendingPages[_pageIndex];
        var hDib = BitmapToDib(path);
        *(IntPtr*)pData = hDib;

        _state = 6; // transferring
        _status.ConditionCode = TWCC.SUCCESS;
        return TWRC.XFERDONE;
    }

    private static unsafe ushort HandleEndXfer(void* pData)
    {
        var pxfers = (TW_PENDINGXFERS*)pData;
        _pageIndex++;
        int remaining = (_pendingPages?.Length ?? 0) - _pageIndex;
        pxfers->Count = (ushort)Math.Max(0, remaining);

        if (remaining <= 0)
        {
            // All pages transferred — commit cursor
            Queue.CommitCursor(_active!, _newCursor);
            pxfers->Count = 0;
            _state = 5;
            Logger.Info("EndXfer: all pages done, cursor committed");
        }

        _status.ConditionCode = TWCC.SUCCESS;
        return TWRC.SUCCESS;
    }

    private static unsafe ushort HandleStopFeeder(void* pData)
    {
        var pxfers = (TW_PENDINGXFERS*)pData;
        pxfers->Count = 0;
        _pageIndex = _pendingPages?.Length ?? 0;
        _state = 5;
        _status.ConditionCode = TWCC.SUCCESS;
        return TWRC.SUCCESS;
    }

    private static ushort Unsupported(uint dg, ushort dat, ushort msg)
    {
        Logger.Warn($"Unsupported: DG=0x{dg:X4} DAT=0x{dat:X4} MSG=0x{msg:X4}");
        _status.ConditionCode = TWCC.BADPROTOCOL;
        return TWRC.FAILURE;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static byte[] BitmapToRgb24(System.Drawing.Bitmap bmp)
    {
        int w = bmp.Width, h = bmp.Height;
        var data = new byte[w * h * 3];
        int idx = 0;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                var c = bmp.GetPixel(x, y);
                data[idx++] = c.R;
                data[idx++] = c.G;
                data[idx++] = c.B;
            }
        }
        return data;
    }

    [DllImport("kernel32.dll")]
    private static extern IntPtr GlobalAlloc(uint uFlags, nuint dwBytes);
    [DllImport("kernel32.dll")]
    private static extern IntPtr GlobalLock(IntPtr h);
    [DllImport("kernel32.dll")]
    private static extern bool GlobalUnlock(IntPtr h);

    private static IntPtr BitmapToDib(string path)
    {
        using var bmp = new System.Drawing.Bitmap(path);
        // Build a DIB (Device-Independent Bitmap) in global memory
        int w = bmp.Width, h = bmp.Height;
        int stride = ((w * 24 + 31) / 32) * 4;
        int pixelBytes = stride * h;
        int headerSize = 40; // BITMAPINFOHEADER
        var totalSize = (nuint)(headerSize + pixelBytes);
        var hDib = GlobalAlloc(0x0002, totalSize); // GMEM_MOVEABLE
        var p = GlobalLock(hDib);

        // BITMAPINFOHEADER
        Marshal.WriteInt32(p, 0, headerSize);
        Marshal.WriteInt32(p, 4, w);
        Marshal.WriteInt32(p, 8, -h); // top-down
        Marshal.WriteInt16(p, 12, 1);  // planes
        Marshal.WriteInt16(p, 14, 24); // bitcount
        Marshal.WriteInt32(p, 16, 0);  // compression = BI_RGB
        Marshal.WriteInt32(p, 20, pixelBytes);
        Marshal.WriteInt32(p, 24, 0);
        Marshal.WriteInt32(p, 28, 0);
        Marshal.WriteInt32(p, 32, 0);
        Marshal.WriteInt32(p, 36, 0);

        // Pixel data
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                var c = bmp.GetPixel(x, y);
                int offset = headerSize + y * stride + x * 3;
                Marshal.WriteByte(p, offset,     c.B);
                Marshal.WriteByte(p, offset + 1, c.G);
                Marshal.WriteByte(p, offset + 2, c.R);
            }
        }

        GlobalUnlock(hDib);
        return hDib;
    }
}
