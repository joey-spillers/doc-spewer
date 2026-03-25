using OneView.VirtualScanner.Core.Models;
using OneView.VirtualScanner.Core.Services;

namespace OneView.VirtualScanner.Manager.Pages;

public class CurrentJobPage : UserControl, IRefreshable
{
    private readonly ProfileManager _pm = new();
    private readonly Label _lblProfile     = new() { AutoSize = false, Height = 24 };
    private readonly Label _lblPdf         = new() { AutoSize = false, Height = 24 };
    private readonly Label _lblTotal       = new() { AutoSize = false, Height = 24 };
    private readonly Label _lblEmit        = new() { AutoSize = false, Height = 24 };
    private readonly Label _lblCursor      = new() { AutoSize = false, Height = 24 };
    private readonly Label _lblLastScan    = new() { AutoSize = false, Height = 24 };
    private readonly Label _lblLastApp     = new() { AutoSize = false, Height = 24 };
    private readonly Label _lblLastResult  = new() { AutoSize = false, Height = 24 };
    private readonly FlowLayoutPanel _thumbs = new()
    {
        FlowDirection = FlowDirection.LeftToRight,
        WrapContents = true,
        AutoScroll = true,
        Dock = DockStyle.Fill,
        BackColor = Color.FromArgb(235, 235, 238)
    };

    private readonly Button _btnReset    = new() { Text = "Reset Cursor",   Width = 120 };
    private readonly Button _btnPause    = new() { Text = "Pause Source",   Width = 120 };
    private readonly Button _btnReload   = new() { Text = "Reload PDF",     Width = 120 };
    private readonly Button _btnPreview  = new() { Text = "Preview Pages",  Width = 120 };

    public CurrentJobPage()
    {
        BackColor = Color.FromArgb(245, 245, 247);
        BuildLayout();
        _btnReset.Click   += (_, _) => DoResetCursor();
        _btnPause.Click   += (_, _) => TogglePause();
        _btnReload.Click  += (_, _) => { LoadData(); LoadThumbnails(); };
        _btnPreview.Click += (_, _) => LoadThumbnails();
    }

    public new void Refresh() => LoadData();

    private void LoadData()
    {
        var active  = _pm.LoadActiveConfig();
        var profiles = _pm.LoadAll();
        var profile = profiles.FirstOrDefault(p => p.Name == active.ActiveProfile);

        _lblProfile.Text    = active.ActiveProfile.Length > 0 ? active.ActiveProfile : "(none)";
        _lblPdf.Text        = profile?.PdfPath ?? "-";
        _lblCursor.Text     = active.Cursor.ToString();
        _lblLastScan.Text   = active.LastScanTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-";
        _lblLastApp.Text    = active.LastCallingApp.Length > 0 ? active.LastCallingApp : "-";
        _lblLastResult.Text = active.LastTransferResult.Length > 0 ? active.LastTransferResult : "-";
        _btnPause.Text      = active.Paused ? "Resume Source" : "Pause Source";

        if (profile != null)
        {
            int total = PdfRenderer.GetPageCount(profile.PdfPath);
            _lblTotal.Text = total.ToString();
            var indices = PdfRenderer.ResolvePagesToEmit(profile, active.Cursor, total);
            _lblEmit.Text  = indices.Length.ToString();
        }
        else
        {
            _lblTotal.Text = "-";
            _lblEmit.Text  = "-";
        }
    }

    private void LoadThumbnails()
    {
        _thumbs.Controls.Clear();
        var active  = _pm.LoadActiveConfig();
        var profiles = _pm.LoadAll();
        var profile = profiles.FirstOrDefault(p => p.Name == active.ActiveProfile);
        if (profile == null || !File.Exists(profile.PdfPath)) return;

        int total   = PdfRenderer.GetPageCount(profile.PdfPath);
        var indices = PdfRenderer.ResolvePagesToEmit(profile, active.Cursor, total);

        foreach (var idx in indices)
        {
            try
            {
                var bmp = PdfRenderer.RenderThumbnail(profile.PdfPath, idx, 110);
                var box = new PictureBox
                {
                    Image = bmp,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Width = 120, Height = 160,
                    Margin = new Padding(4),
                    BorderStyle = BorderStyle.FixedSingle
                };
                var lbl = new Label
                {
                    Text = $"Page {idx + 1}",
                    Width = 120, Height = 18,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 8f)
                };
                var wrap = new Panel { Width = 128, Height = 186, Margin = new Padding(4) };
                wrap.Controls.Add(box);
                lbl.Top = 162; lbl.Left = 0;
                wrap.Controls.Add(lbl);
                _thumbs.Controls.Add(wrap);
            }
            catch { /* skip unrenderable pages */ }
        }
    }

    private void DoResetCursor()
    {
        var active = _pm.LoadActiveConfig();
        active.Cursor = 0;
        _pm.SaveActiveConfig(active);
        LoadData();
    }

    private void TogglePause()
    {
        var active = _pm.LoadActiveConfig();
        active.Paused = !active.Paused;
        _pm.SaveActiveConfig(active);
        LoadData();
    }

    private void BuildLayout()
    {
        var title = new Label
        {
            Text = "Current Job",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Dock = DockStyle.Top, Height = 36,
            Padding = new Padding(12, 4, 0, 0)
        };
        Controls.Add(title);

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterDistance = 320,
            Orientation = Orientation.Vertical
        };
        Controls.Add(split);

        // Left: info + buttons
        var info = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Padding = new Padding(12)
        };
        info.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        info.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        split.Panel1.Controls.Add(info);

        void AddInfo(string label, Label val)
        {
            info.Controls.Add(new Label
            {
                Text = label, Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Height = 24, TextAlign = ContentAlignment.MiddleRight
            });
            val.Dock = DockStyle.None;
            val.Width = 200;
            info.Controls.Add(val);
        }

        AddInfo("Active Profile:",    _lblProfile);
        AddInfo("PDF:",               _lblPdf);
        AddInfo("Total Pages:",       _lblTotal);
        AddInfo("Pages This Scan:",   _lblEmit);
        AddInfo("Cursor:",            _lblCursor);
        AddInfo("Last Scan:",         _lblLastScan);
        AddInfo("Last App:",          _lblLastApp);
        AddInfo("Last Result:",       _lblLastResult);

        var btnRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom, Height = 40,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(8, 4, 0, 0)
        };
        foreach (var b in new[] { _btnPreview, _btnReset, _btnPause, _btnReload })
        {
            b.Height = 30;
            btnRow.Controls.Add(b);
        }
        split.Panel1.Controls.Add(btnRow);

        // Right: thumbnails
        var thumbTitle = new Label
        {
            Text = "Next Scan Preview",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Dock = DockStyle.Top, Height = 24
        };
        split.Panel2.Controls.Add(_thumbs);
        split.Panel2.Controls.Add(thumbTitle);

        LoadData();
    }
}
