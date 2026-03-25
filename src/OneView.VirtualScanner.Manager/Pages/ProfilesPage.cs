using OneView.VirtualScanner.Core.Models;
using OneView.VirtualScanner.Core.Services;

namespace OneView.VirtualScanner.Manager.Pages;

public class ProfilesPage : UserControl
{
    private readonly ProfileManager _pm = new();
    private List<ScanProfile> _profiles = new();
    private ScanProfile? _editing;

    // Left: list
    private readonly ListBox _list = new();
    private readonly Button _btnNew     = new() { Text = "New",    Width = 72 };
    private readonly Button _btnClone   = new() { Text = "Clone",  Width = 72 };
    private readonly Button _btnDelete  = new() { Text = "Delete", Width = 72 };
    private readonly Button _btnActivate = new() { Text = "Set Active", Width = 80 };

    // Right: editor
    private readonly TextBox _name        = new() { Width = 260 };
    private readonly TextBox _pdfPath     = new() { Width = 220 };
    private readonly Button  _browse      = new() { Text = "...", Width = 32 };
    private readonly NumericUpDown _pageCount = new() { Minimum=1, Maximum=9999, Value=5, Width=70 };
    private readonly NumericUpDown _startPage = new() { Minimum=1, Maximum=9999, Value=1, Width=70 };
    private readonly ComboBox _pageMode   = new() { DropDownStyle=ComboBoxStyle.DropDownList, Width=150 };
    private readonly ComboBox _loopMode   = new() { DropDownStyle=ComboBoxStyle.DropDownList, Width=150 };
    private readonly ComboBox _pixelType  = new() { DropDownStyle=ComboBoxStyle.DropDownList, Width=150 };
    private readonly ComboBox _feederMode = new() { DropDownStyle=ComboBoxStyle.DropDownList, Width=150 };
    private readonly ComboBox _paperSize  = new() { DropDownStyle=ComboBoxStyle.DropDownList, Width=150 };
    private readonly NumericUpDown _dpi   = new() { Minimum=72, Maximum=1200, Value=300, Width=70 };
    private readonly CheckBox _duplex     = new() { Text = "Duplex" };
    private readonly CheckBox _showUi     = new() { Text = "Show UI on scan" };
    private readonly NumericUpDown _delay = new() { Minimum=0, Maximum=5000, Value=0, Width=70 };
    private readonly ComboBox _errorSim   = new() { DropDownStyle=ComboBoxStyle.DropDownList, Width=150 };
    private readonly Label _activeLabel   = new() { ForeColor=Color.DodgerBlue, Font=new Font("Segoe UI",9,FontStyle.Bold) };
    private readonly Button _btnSave      = new() { Text = "Save Profile", Width = 110 };

    public ProfilesPage()
    {
        BackColor = Color.FromArgb(245, 245, 247);
        BuildLayout();
        PopulateCombos();
        LoadProfiles();
        _list.SelectedIndexChanged += (_, _) => LoadSelected();
        _browse.Click += BrowsePdf;
        _btnNew.Click     += (_, _) => NewProfile();
        _btnClone.Click   += (_, _) => CloneProfile();
        _btnDelete.Click  += (_, _) => DeleteProfile();
        _btnActivate.Click += (_, _) => ActivateCurrent();
        _btnSave.Click    += (_, _) => SaveCurrent();
    }

    // ── Public API (called from MainForm toolbar) ───────────────────────────
    public void NewProfile()
    {
        _editing = new ScanProfile { Name = "New Profile" };
        PopulateEditor(_editing);
        _name.Focus();
        _name.SelectAll();
    }

    public void SaveCurrent()
    {
        if (_editing == null) return;
        PullEditorInto(_editing);
        _pm.Save(_editing);
        LoadProfiles(selectName: _editing.Name);
    }

    public void ActivateCurrent()
    {
        if (_editing == null) return;
        PullEditorInto(_editing);
        _pm.Save(_editing);

        var active = _pm.LoadActiveConfig();
        active.ActiveProfile = _editing.Name;
        active.Cursor = 0;
        _pm.SaveActiveConfig(active);
        LoadProfiles(selectName: _editing.Name);
        _activeLabel.Text = $"Active: {_editing.Name}";
        MessageBox.Show($"Profile '{_editing.Name}' is now active.", "Activated",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // ── Private ─────────────────────────────────────────────────────────────
    private void LoadProfiles(string? selectName = null)
    {
        _profiles = _pm.LoadAll();
        var active = _pm.LoadActiveConfig();
        _activeLabel.Text = string.IsNullOrEmpty(active.ActiveProfile)
            ? "(no active profile)"
            : $"Active: {active.ActiveProfile}";

        _list.Items.Clear();
        foreach (var p in _profiles)
            _list.Items.Add(p.Name + (p.Name == active.ActiveProfile ? " ✓" : ""));

        if (selectName != null)
        {
            for (int i = 0; i < _profiles.Count; i++)
                if (_profiles[i].Name == selectName) { _list.SelectedIndex = i; return; }
        }
        if (_list.Items.Count > 0) _list.SelectedIndex = 0;
    }

    private void LoadSelected()
    {
        var idx = _list.SelectedIndex;
        if (idx < 0 || idx >= _profiles.Count) return;
        _editing = _profiles[idx];
        PopulateEditor(_editing);
    }

    private void CloneProfile()
    {
        if (_editing == null) return;
        var clone = _editing.Clone();
        clone.Name = _editing.Name + " (copy)";
        _editing = clone;
        PopulateEditor(_editing);
        _name.Focus();
        _name.SelectAll();
    }

    private void DeleteProfile()
    {
        if (_editing == null) return;
        if (MessageBox.Show($"Delete '{_editing.Name}'?", "Confirm",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        _pm.Delete(_editing);
        _editing = null;
        LoadProfiles();
    }

    private void BrowsePdf(object? s, EventArgs e)
    {
        using var dlg = new OpenFileDialog
        {
            Title = "Select PDF",
            Filter = "PDF files|*.pdf|All files|*.*"
        };
        if (dlg.ShowDialog() == DialogResult.OK)
            _pdfPath.Text = dlg.FileName;
    }

    private void PopulateEditor(ScanProfile p)
    {
        _name.Text        = p.Name;
        _pdfPath.Text     = p.PdfPath;
        _pageCount.Value  = p.PageCount;
        _startPage.Value  = p.StartPage;
        SetCombo(_pageMode,   p.PageMode.ToString());
        SetCombo(_loopMode,   p.LoopMode.ToString());
        SetCombo(_pixelType,  p.PixelType.ToString());
        SetCombo(_feederMode, p.FeederMode.ToString());
        SetCombo(_paperSize,  p.PaperSize);
        _dpi.Value     = Math.Max(72, Math.Min(1200, p.Dpi));
        _duplex.Checked  = p.Duplex;
        _showUi.Checked  = p.ShowUi;
        _delay.Value   = Math.Min(5000, p.InterPageDelayMs);
        SetCombo(_errorSim, p.ErrorSimulation.ToString());
    }

    private void PullEditorInto(ScanProfile p)
    {
        p.Name        = _name.Text.Trim();
        p.PdfPath     = _pdfPath.Text.Trim();
        p.PageCount   = (int)_pageCount.Value;
        p.StartPage   = (int)_startPage.Value;
        p.PageMode    = Enum.Parse<PageMode>(_pageMode.SelectedItem?.ToString() ?? "FirstN");
        p.LoopMode    = Enum.Parse<LoopMode>(_loopMode.SelectedItem?.ToString() ?? "Restart");
        p.PixelType   = Enum.Parse<PixelType>(_pixelType.SelectedItem?.ToString() ?? "Color");
        p.FeederMode  = Enum.Parse<FeederMode>(_feederMode.SelectedItem?.ToString() ?? "ADF");
        p.PaperSize   = _paperSize.SelectedItem?.ToString() ?? "Letter";
        p.Dpi         = (int)_dpi.Value;
        p.Duplex      = _duplex.Checked;
        p.ShowUi      = _showUi.Checked;
        p.InterPageDelayMs = (int)_delay.Value;
        p.ErrorSimulation  = Enum.Parse<ErrorSimulation>(_errorSim.SelectedItem?.ToString() ?? "None");
    }

    private static void SetCombo(ComboBox cb, string val)
    {
        for (int i = 0; i < cb.Items.Count; i++)
            if (cb.Items[i]?.ToString() == val) { cb.SelectedIndex = i; return; }
        if (cb.Items.Count > 0) cb.SelectedIndex = 0;
    }

    private void PopulateCombos()
    {
        foreach (var v in Enum.GetNames<PageMode>())   _pageMode.Items.Add(v);
        foreach (var v in Enum.GetNames<LoopMode>())   _loopMode.Items.Add(v);
        foreach (var v in Enum.GetNames<PixelType>())  _pixelType.Items.Add(v);
        foreach (var v in Enum.GetNames<FeederMode>()) _feederMode.Items.Add(v);
        foreach (var v in new[] { "Letter","Legal","A4","Auto" }) _paperSize.Items.Add(v);
        foreach (var v in Enum.GetNames<ErrorSimulation>()) _errorSim.Items.Add(v);
        _pageMode.SelectedIndex = 1; _loopMode.SelectedIndex = 1;
        _pixelType.SelectedIndex = 0; _feederMode.SelectedIndex = 1;
        _paperSize.SelectedIndex = 0; _errorSim.SelectedIndex = 0;
    }

    private void BuildLayout()
    {
        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Panel1MinSize = 180,
            Panel2MinSize = 300
        };
        Controls.Add(split);
        split.SplitterDistance = 230;

        // ── Left: list panel ────────────────────────────────────────────────
        var leftPad = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8) };
        split.Panel1.Controls.Add(leftPad);

        var listLabel = new Label { Text = "Profiles", Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 10, FontStyle.Bold), Height = 28 };
        leftPad.Controls.Add(_list);
        leftPad.Controls.Add(listLabel);

        _list.Dock = DockStyle.Fill;
        _list.Font = new Font("Segoe UI", 9.5f);
        _list.BorderStyle = BorderStyle.FixedSingle;

        var btnBar = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 36,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0,4,0,0)
        };
        foreach (var b in new[] { _btnNew, _btnClone, _btnDelete, _btnActivate })
        {
            b.Height = 28;
            btnBar.Controls.Add(b);
        }
        leftPad.Controls.Add(btnBar);

        _activeLabel.Dock = DockStyle.Bottom;
        _activeLabel.Height = 20;
        leftPad.Controls.Add(_activeLabel);

        // ── Right: editor ───────────────────────────────────────────────────
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(16) };
        split.Panel2.Controls.Add(scroll);

        var tbl = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 2,
            Padding = new Padding(0)
        };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        scroll.Controls.Add(tbl);

        void AddRow(string label, Control ctrl, bool fullWidth = false)
        {
            tbl.Controls.Add(new Label { Text = label, TextAlign = ContentAlignment.MiddleRight,
                AutoSize = false, Width = 155, Height = 28, Font = new Font("Segoe UI", 9f) });
            if (fullWidth)
            {
                var row = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight };
                row.Controls.Add(ctrl);
                tbl.Controls.Add(row);
            }
            else
                tbl.Controls.Add(ctrl);
        }

        var pdfRow = new FlowLayoutPanel { AutoSize = true };
        pdfRow.Controls.Add(_pdfPath);
        pdfRow.Controls.Add(_browse);

        tbl.Controls.Add(new Label { Text = "Profile Editor",
            Font = new Font("Segoe UI", 11, FontStyle.Bold), Height = 32,
            TextAlign = ContentAlignment.MiddleLeft, AutoSize = false, Width = 300 });
        tbl.Controls.Add(new Label());

        AddRow("Name",             _name);
        AddRow("PDF Path",         pdfRow);
        AddRow("Page Mode",        _pageMode);
        AddRow("Pages to Emit",    _pageCount);
        AddRow("Start Page",       _startPage);
        AddRow("Loop Mode",        _loopMode);
        AddRow("Pixel Type",       _pixelType);
        AddRow("Feeder Mode",      _feederMode);
        AddRow("Paper Size",       _paperSize);
        AddRow("DPI",              _dpi);
        AddRow("",                 _duplex);
        AddRow("",                 _showUi);
        AddRow("Inter-page Delay", _delay);
        AddRow("Error Simulation", _errorSim);
        AddRow("",                 _btnSave);
    }
}
