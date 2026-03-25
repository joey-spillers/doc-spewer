using System.Text.Json.Serialization;

namespace OneView.VirtualScanner.Core.Models;

public enum PageMode { AllPages, Range, FirstN, ExplicitList }
public enum LoopMode { Stop, Restart, Continue }
public enum PixelType { Color, Grayscale, BlackAndWhite }
public enum FeederMode { Flatbed, ADF }
public enum ErrorSimulation { None, PaperJam, CoverOpen, NoPaper, TransferCanceled }

public class ScanProfile
{
    public string Name { get; set; } = "New Profile";
    public string PdfPath { get; set; } = string.Empty;
    public PageMode PageMode { get; set; } = PageMode.FirstN;
    public int PageCount { get; set; } = 5;
    public int StartPage { get; set; } = 1;
    public string ExplicitPages { get; set; } = string.Empty;
    public LoopMode LoopMode { get; set; } = LoopMode.Restart;
    public bool Duplex { get; set; } = false;
    public FeederMode FeederMode { get; set; } = FeederMode.ADF;
    public int Dpi { get; set; } = 300;
    public PixelType PixelType { get; set; } = PixelType.Color;
    public bool ShowUi { get; set; } = false;
    public int InterPageDelayMs { get; set; } = 0;
    public ErrorSimulation ErrorSimulation { get; set; } = ErrorSimulation.None;
    public string PaperSize { get; set; } = "Letter";
    public bool InjectBlanks { get; set; } = false;
    public int BlankEveryN { get; set; } = 0;

    // Device identity overrides (null = use Behavior page defaults)
    public string? SourceNameOverride { get; set; }

    [JsonIgnore]
    public bool IsActive { get; set; }

    public ScanProfile Clone() =>
        System.Text.Json.JsonSerializer.Deserialize<ScanProfile>(
            System.Text.Json.JsonSerializer.Serialize(this))!;
}
