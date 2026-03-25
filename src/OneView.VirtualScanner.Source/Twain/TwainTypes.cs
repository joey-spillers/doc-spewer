using System.Runtime.InteropServices;

namespace OneView.VirtualScanner.Source.Twain;

// ── Primitive typedefs ───────────────────────────────────────────────────────
using TW_BOOL    = System.UInt16;
using TW_FIX32I  = System.Int16;
using TW_FIX32W  = System.UInt16;
using TW_INT8    = System.SByte;
using TW_INT16   = System.Int16;
using TW_INT32   = System.Int32;
using TW_UINT8   = System.Byte;
using TW_UINT16  = System.UInt16;
using TW_UINT32  = System.UInt32;
using TW_HANDLE  = System.IntPtr;
using TW_MEMREF  = System.IntPtr;
using TW_UINTPTR = System.UIntPtr;

[StructLayout(LayoutKind.Sequential, Pack = 2)]
public struct TW_FIX32
{
    public TW_INT16  Whole;
    public TW_FIX32W Frac;

    public static TW_FIX32 FromDouble(double v)
    {
        return new TW_FIX32 { Whole = (TW_INT16)v, Frac = (TW_FIX32W)((v - (int)v) * 65536.0) };
    }
    public double ToDouble() => Whole + Frac / 65536.0;
}

[StructLayout(LayoutKind.Sequential, Pack = 2, CharSet = CharSet.Ansi)]
public struct TW_IDENTITY
{
    public TW_UINT32  Id;
    public TW_VERSION Version;
    public TW_UINT16  ProtocolMajor;
    public TW_UINT16  ProtocolMinor;
    public TW_UINT32  SupportedGroups;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 34)]
    public string     Manufacturer;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 34)]
    public string     ProductFamily;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 34)]
    public string     ProductName;
}

[StructLayout(LayoutKind.Sequential, Pack = 2, CharSet = CharSet.Ansi)]
public struct TW_VERSION
{
    public TW_UINT16 MajorNum;
    public TW_UINT16 MinorNum;
    public TW_UINT16 Language;
    public TW_UINT16 Country;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 34)]
    public string    Info;
}

[StructLayout(LayoutKind.Sequential, Pack = 2)]
public struct TW_CAPABILITY
{
    public TW_UINT16 Cap;
    public TW_UINT16 ConType;
    public TW_HANDLE hContainer;
}

[StructLayout(LayoutKind.Sequential, Pack = 2)]
public struct TW_IMAGEINFO
{
    public TW_FIX32  XResolution;
    public TW_FIX32  YResolution;
    public TW_INT32  ImageWidth;
    public TW_INT32  ImageLength;
    public TW_INT16  SamplesPerPixel;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public TW_INT16[] BitsPerSample;
    public TW_INT16  BitsPerPixel;
    public TW_BOOL   Planar;
    public TW_INT16  PixelType;
    public TW_UINT16 Compression;
}

[StructLayout(LayoutKind.Sequential, Pack = 2)]
public struct TW_SETUPMEMXFER
{
    public TW_UINT32 MinBufSize;
    public TW_UINT32 MaxBufSize;
    public TW_UINT32 Preferred;
}

[StructLayout(LayoutKind.Sequential, Pack = 2)]
public struct TW_IMAGEMEMXFER
{
    public TW_UINT16 Compression;
    public TW_UINT32 BytesPerRow;
    public TW_UINT32 Columns;
    public TW_UINT32 Rows;
    public TW_UINT32 XOffset;
    public TW_UINT32 YOffset;
    public TW_UINT32 BytesWritten;
    public TW_HANDLE Memory_Flags;
    public TW_UINT32 Memory_Length;
    public TW_MEMREF Memory_TheMem;
}

[StructLayout(LayoutKind.Sequential, Pack = 2)]
public struct TW_STATUS
{
    public TW_UINT16 ConditionCode;
    public TW_UINT16 Reserved;
}

[StructLayout(LayoutKind.Sequential, Pack = 2)]
public struct TW_USERINTERFACE
{
    public TW_BOOL   ShowUI;
    public TW_BOOL   ModalUI;
    public TW_HANDLE hParent;
}

[StructLayout(LayoutKind.Sequential, Pack = 2)]
public struct TW_PENDINGXFERS
{
    public TW_UINT16 Count;
    public TW_UINT32 EOJ;
}

[StructLayout(LayoutKind.Sequential, Pack = 2)]
public struct TW_ONEVALUE
{
    public TW_UINT16 ItemType;
    public TW_UINT32 Item;
}

[StructLayout(LayoutKind.Sequential, Pack = 2)]
public unsafe struct TW_ENUMERATION
{
    public TW_UINT16 ItemType;
    public TW_UINT32 NumItems;
    public TW_UINT32 CurrentIndex;
    public TW_UINT32 DefaultIndex;
    public fixed byte ItemList[4]; // variable length — allocate separately
}
