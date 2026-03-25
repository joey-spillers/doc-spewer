using OneView.VirtualScanner.Core.Models;
using OneView.VirtualScanner.Core.Services;

namespace OneView.VirtualScanner.Manager.Pages;

public class BehaviorPage : UserControl, IRefreshable
{
    private readonly ProfileManager _pm = new();

    // Device identity
    private readonly TextBox _sourceName    = new() { Width = 240 };
    private readonly TextBox _manufacturer  = new() { Width = 240 };
    private readonly TextBox _productFamily = new() { Width = 240 };
    private readonly TextBox _serial        = new() { Width = 160 };
    private readonly TextBox _version       = new() { Width = 80 };

    // Scan behavior
    private readonly CheckBox _advAdf    = new() { Text = "Advertise ADF" };
    private readonly CheckBox _advDuplex = new() { Text = "Advertise Duplex" };
    private readonly CheckBox _openUi    = new() { Text = "Open UI on acquire" };
    private readonly CheckBox _autoClose = new() { Text = "Auto-close after scan" };
    private readonly CheckBox _feederAlways = new() { Text = "Feeder always loaded" };

    // Image processing
    private readonly CheckBox _rotate    = new() { Text = "Rotate pages" };
    private readonly CheckBox _deskew    = new() { Text = "Deskew" };
    private readonly CheckBox _noise     = new() { Text = "Add noise" };
    private readonly CheckBox _streak    = new() { Text = "Fake streaking" };
    private readonly NumericUpDown _brightness = new() { Minimum=-100, Maximum=100, Value=0, Width=70 };
    private readonly NumericUpDown _contrast   = new() { Minimum=-100, Maximum=100, Value=0, Width=70 };

    private readonly Button _btnSave = new() { Text = "Save Behavior Settings", Width = 200 };

    public BehaviorPage()
    {
        BackColor = Color.FromArgb(245, 245, 247);
        BuildLayout();
        _btnSave.Click += (_, _) => Save();
        LoadData();
    }

    public new void Refresh() => LoadData();

    private void LoadData()
    {
        var b = _pm.LoadBehavior();
        _sourceName.Text    = b.SourceName;
        _manufacturer.Text  = b.Manufacturer;
        _productFamily.Text = b.ProductFamily;
        _serial.Text        = b.DeviceSerial;
        _version.Text       = b.Version;
        _advAdf.Checked     = b.AdvertiseAdf;
        _advDuplex.Checked  = b.AdvertiseDuplex;
        _openUi.Checked     = b.OpenSourceUiOnAcquire;
        _autoClose.Checked  = b.AutoCloseAfterScan;
        _feederAlways.Checked = b.FeederAlwaysLoaded;
        _rotate.Checked     = b.RotatePages;
        _deskew.Checked     = b.Deskew;
        _noise.Checked      = b.AddNoise;
        _streak.Checked     = b.FakeStreaking;
        _brightness.Value   = b.Brightness;
        _contrast.Value     = b.Contrast;
    }

    private void Save()
    {
        var b = new BehaviorSettings
        {
            SourceName    = _sourceName.Text.Trim(),
            Manufacturer  = _manufacturer.Text.Trim(),
            ProductFamily = _productFamily.Text.Trim(),
            DeviceSerial  = _serial.Text.Trim(),
            Version       = _version.Text.Trim(),
            AdvertiseAdf  = _advAdf.Checked,
            AdvertiseDuplex = _advDuplex.Checked,
            OpenSourceUiOnAcquire = _openUi.Checked,
            AutoCloseAfterScan = _autoClose.Checked,
            FeederAlwaysLoaded = _feederAlways.Checked,
            RotatePages   = _rotate.Checked,
            Deskew        = _deskew.Checked,
            AddNoise      = _noise.Checked,
            FakeStreaking = _streak.Checked,
            Brightness    = (int)_brightness.Value,
            Contrast      = (int)_contrast.Value
        };
        _pm.SaveBehavior(b);
        MessageBox.Show("Behavior settings saved.", "Saved",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void BuildLayout()
    {
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(16) };
        Controls.Add(scroll);

        var tbl = new TableLayoutPanel { AutoSize = true, ColumnCount = 2 };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        scroll.Controls.Add(tbl);

        void Section(string text)
        {
            var lbl = new Label
            {
                Text = text, Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Height = 32, TextAlign = ContentAlignment.BottomLeft,
                AutoSize = false
            };
            tbl.Controls.Add(lbl);
            tbl.SetColumnSpan(lbl, 2);
            tbl.Controls.Add(new Label());
        }

        void Row(string label, Control ctrl)
        {
            tbl.Controls.Add(new Label
            {
                Text = label, Font = new Font("Segoe UI", 9f),
                Height = 28, TextAlign = ContentAlignment.MiddleRight,
                AutoSize = false
            });
            tbl.Controls.Add(ctrl);
        }

        Section("Device Identity");
        Row("Source Name",     _sourceName);
        Row("Manufacturer",    _manufacturer);
        Row("Product Family",  _productFamily);
        Row("Serial Number",   _serial);
        Row("Version",         _version);

        Section("Scan Behavior");
        tbl.Controls.Add(_advAdf);    tbl.SetColumnSpan(_advAdf, 2);
        tbl.Controls.Add(_advDuplex); tbl.SetColumnSpan(_advDuplex, 2);
        tbl.Controls.Add(_openUi);    tbl.SetColumnSpan(_openUi, 2);
        tbl.Controls.Add(_autoClose); tbl.SetColumnSpan(_autoClose, 2);
        tbl.Controls.Add(_feederAlways); tbl.SetColumnSpan(_feederAlways, 2);

        Section("Image Processing");
        tbl.Controls.Add(_rotate);    tbl.SetColumnSpan(_rotate, 2);
        tbl.Controls.Add(_deskew);    tbl.SetColumnSpan(_deskew, 2);
        tbl.Controls.Add(_noise);     tbl.SetColumnSpan(_noise, 2);
        tbl.Controls.Add(_streak);    tbl.SetColumnSpan(_streak, 2);
        Row("Brightness", _brightness);
        Row("Contrast",   _contrast);

        tbl.Controls.Add(new Label());
        tbl.Controls.Add(_btnSave);
    }
}
