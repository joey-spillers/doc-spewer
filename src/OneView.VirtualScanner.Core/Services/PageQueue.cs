using OneView.VirtualScanner.Core.Models;

namespace OneView.VirtualScanner.Core.Services;

public class PageQueue
{
    private readonly ProfileManager _pm = new();

    /// <summary>
    /// Returns ordered page file paths for the next scan, advancing the cursor if LoopMode.Continue.
    /// Call CommitCursor() after a successful scan.
    /// </summary>
    public (string[] Pages, int NewCursor) PrepareNextScan(ScanProfile profile, int cursor)
    {
        int total = PdfRenderer.GetPageCount(profile.PdfPath);
        if (total == 0) return (Array.Empty<string>(), cursor);

        var indices = PdfRenderer.ResolvePagesToEmit(profile, cursor, total);
        if (indices.Length == 0) return (Array.Empty<string>(), cursor);

        var cacheDir = Path.Combine(SharedPaths.PageCacheDir, MakeSafeName(profile.Name));
        var pages = PdfRenderer.RenderPages(profile, indices, cacheDir);

        int newCursor = profile.LoopMode switch
        {
            LoopMode.Continue => (cursor + indices.Length) % total,
            LoopMode.Restart => 0,
            LoopMode.Stop => cursor + indices.Length,
            _ => 0
        };

        return (pages.ToArray(), newCursor);
    }

    public void CommitCursor(ActiveConfig active, int newCursor)
    {
        active.Cursor = newCursor;
        active.LastScanTime = DateTime.Now;
        _pm.SaveActiveConfig(active);
    }

    private static string MakeSafeName(string name) =>
        string.Concat(name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
}
