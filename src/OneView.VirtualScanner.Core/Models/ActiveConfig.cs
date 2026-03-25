namespace OneView.VirtualScanner.Core.Models;

public class ActiveConfig
{
    public string ActiveProfile { get; set; } = string.Empty;
    public int Cursor { get; set; } = 0;
    public DateTime? LastScanTime { get; set; }
    public string LastCallingApp { get; set; } = string.Empty;
    public string LastTransferResult { get; set; } = string.Empty;
    public bool Paused { get; set; } = false;
}
