using System.Runtime.InteropServices;
using OneView.VirtualScanner.Source.Twain;

namespace OneView.VirtualScanner.Source.Capabilities;

/// <summary>
/// Helpers for allocating TWAIN capability containers via GlobalAlloc.
/// </summary>
internal static class CapabilityHelper
{
    [DllImport("kernel32.dll")]
    private static extern IntPtr GlobalAlloc(uint uFlags, nuint dwBytes);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GlobalLock(IntPtr hMem);

    [DllImport("kernel32.dll")]
    private static extern bool GlobalUnlock(IntPtr hMem);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GlobalFree(IntPtr hMem);

    private const uint GMEM_MOVEABLE = 0x0002;

    /// <summary>Allocates a TW_ONEVALUE container and returns the handle.</summary>
    public static IntPtr AllocOneValue(ushort itemType, uint item)
    {
        var size = (nuint)(Marshal.SizeOf<TW_ONEVALUE>());
        var h = GlobalAlloc(GMEM_MOVEABLE, size);
        var p = GlobalLock(h);
        try
        {
            Marshal.WriteInt16(p, 0, (short)itemType);
            Marshal.WriteInt32(p, 2, (int)item);
        }
        finally { GlobalUnlock(h); }
        return h;
    }

    /// <summary>Allocates a TW_ENUMERATION container for ushort items.</summary>
    public static IntPtr AllocEnumeration(ushort itemType, ushort[] items,
        uint currentIndex, uint defaultIndex)
    {
        // Header: ItemType(2) + NumItems(4) + CurrentIndex(4) + DefaultIndex(4) = 14 bytes
        // Each UINT16 item = 2 bytes
        int headerSize = 14;
        int itemSize = 2; // UINT16
        var totalSize = (nuint)(headerSize + items.Length * itemSize);
        var h = GlobalAlloc(GMEM_MOVEABLE, totalSize);
        var p = GlobalLock(h);
        try
        {
            Marshal.WriteInt16(p, 0, (short)itemType);
            Marshal.WriteInt32(p, 2, items.Length);
            Marshal.WriteInt32(p, 6, (int)currentIndex);
            Marshal.WriteInt32(p, 10, (int)defaultIndex);
            for (int i = 0; i < items.Length; i++)
                Marshal.WriteInt16(p, headerSize + i * itemSize, (short)items[i]);
        }
        finally { GlobalUnlock(h); }
        return h;
    }

    /// <summary>Frees a previously allocated handle.</summary>
    public static void FreeHandle(IntPtr h)
    {
        if (h != IntPtr.Zero) GlobalFree(h);
    }
}
