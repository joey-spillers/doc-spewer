namespace OneView.VirtualScanner.Core.Models;

public class BehaviorSettings
{
    public string SourceName { get; set; } = "OneView Virtual Scanner";
    public string Manufacturer { get; set; } = "OneView Labs";
    public string ProductFamily { get; set; } = "Virtual ADF TWAIN";
    public string DeviceSerial { get; set; } = "OV-0001";
    public string Version { get; set; } = "1.0";

    public bool AdvertiseAdf { get; set; } = true;
    public bool AdvertiseDuplex { get; set; } = true;
    public bool OpenSourceUiOnAcquire { get; set; } = false;
    public bool AutoCloseAfterScan { get; set; } = true;
    public bool FeederAlwaysLoaded { get; set; } = true;

    // Image processing
    public bool RotatePages { get; set; } = false;
    public bool Deskew { get; set; } = false;
    public bool AddNoise { get; set; } = false;
    public bool FakeStreaking { get; set; } = false;
    public int Brightness { get; set; } = 0;
    public int Contrast { get; set; } = 0;
}
