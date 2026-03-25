using PDFtoImage;
using SkiaSharp;
using OneView.VirtualScanner.Core.Models;

namespace OneView.VirtualScanner.Core.Services;

public class PdfRenderer
{
    /// <summary>Returns the 0-based page indices to emit for the given profile + cursor.</summary>
    public static int[] ResolvePagesToEmit(ScanProfile profile, int cursor, int totalPages)
    {
        if (totalPages == 0) return Array.Empty<int>();
        var all = Enumerable.Range(0, totalPages).ToArray();

        IEnumerable<int> pool = profile.PageMode switch
        {
            PageMode.AllPages    => all,
            PageMode.FirstN      => all.Take(profile.PageCount),
            PageMode.Range       => all.Skip(profile.StartPage - 1).Take(profile.PageCount),
            PageMode.ExplicitList => ParseExplicit(profile.ExplicitPages, totalPages),
            _                    => all
        };

        var poolArr = pool.ToArray();
        if (poolArr.Length == 0) return Array.Empty<int>();

        int start = cursor % poolArr.Length;
        return poolArr.Skip(start).Take(profile.PageCount).ToArray();
    }

    /// <summary>Renders pages of a PDF at the given DPI into a cache folder. Returns file paths.</summary>
    public static List<string> RenderPages(ScanProfile profile, int[] pageIndices, string cacheDir)
    {
        if (!File.Exists(profile.PdfPath))
            throw new FileNotFoundException("PDF not found", profile.PdfPath);

        Directory.CreateDirectory(cacheDir);
        var pdfBytes = File.ReadAllBytes(profile.PdfPath);
        var opts = new RenderOptions(Dpi: profile.Dpi);
        var results = new List<string>();

        foreach (var idx in pageIndices)
        {
            var outPath = Path.Combine(cacheDir, $"page_{idx:D4}.bmp");
            using var skBmp = Conversion.ToImage(pdfBytes, null, idx, opts);
            using var fs = File.Create(outPath);
            skBmp.Encode(fs, SKEncodedImageFormat.Bmp, 100);
            results.Add(outPath);
        }

        return results;
    }

    /// <summary>Returns total page count for a PDF.</summary>
    public static int GetPageCount(string pdfPath)
    {
        if (!File.Exists(pdfPath)) return 0;
        try
        {
            var bytes = File.ReadAllBytes(pdfPath);
            return Conversion.GetPageCount(bytes, password: null);
        }
        catch { return 0; }
    }

    /// <summary>Renders a single page thumbnail for UI preview.</summary>
    public static System.Drawing.Bitmap RenderThumbnail(string pdfPath, int pageIndex, int thumbWidth = 120)
    {
        var bytes = File.ReadAllBytes(pdfPath);
        var opts = new RenderOptions(Dpi: 72);
        using var skBmp = Conversion.ToImage(bytes, null, pageIndex, opts);

        var targetHeight = (int)(thumbWidth * skBmp.Height / (double)skBmp.Width);
        var resizedInfo   = new SKImageInfo(thumbWidth, targetHeight);

        using var resized = skBmp.Resize(resizedInfo, SKFilterQuality.Low);
        using var encodedMs = new MemoryStream();
        resized!.Encode(encodedMs, SKEncodedImageFormat.Png, 90);
        encodedMs.Seek(0, SeekOrigin.Begin);
        return new System.Drawing.Bitmap(encodedMs);
    }

    private static IEnumerable<int> ParseExplicit(string spec, int total)
    {
        foreach (var tok in spec.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var t = tok.Trim();
            if (t.Contains('-'))
            {
                var parts = t.Split('-');
                if (int.TryParse(parts[0], out int a) && int.TryParse(parts[1], out int b))
                    for (int i = a - 1; i < b && i < total; i++) yield return i;
            }
            else if (int.TryParse(t, out int n) && n >= 1 && n <= total)
                yield return n - 1;
        }
    }
}
