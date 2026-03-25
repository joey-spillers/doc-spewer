using OneView.VirtualScanner.Manager.Controls;
using OneView.VirtualScanner.Manager.Pages;

namespace OneView.VirtualScanner.Manager;

public class MainForm : Form
{
    private readonly NavPanel _nav = new();
    private readonly Panel _content = new() { Dock = DockStyle.Fill };
    private readonly ProfilesPage _profilesPage = new();
    private readonly CurrentJobPage _currentJobPage = new();
    private readonly BehaviorPage _behaviorPage = new();
    private readonly DiagnosticsPage _diagnosticsPage = new();

    public MainForm()
    {
        Text = "OneView Virtual Scanner Manager";
        Size = new Size(1100, 720);
        MinimumSize = new Size(900, 600);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(245, 245, 247);

        BuildToolbar();
        _content.BackColor = Color.FromArgb(245, 245, 247);

        Controls.Add(_content);
        Controls.Add(_nav);

        _nav.PageSelected += OnPageSelected;

        // Register all pages
        foreach (var page in new Control[] { _profilesPage, _currentJobPage, _behaviorPage, _diagnosticsPage })
        {
            page.Dock = DockStyle.Fill;
            page.Visible = false;
            _content.Controls.Add(page);
        }

        ShowPage("Profiles");
    }

    private void BuildToolbar()
    {
        var toolbar = new ToolStrip
        {
            Dock = DockStyle.Top,
            BackColor = Color.FromArgb(55, 55, 65),
            Padding = new Padding(4, 2, 4, 2),
            GripStyle = ToolStripGripStyle.Hidden,
            RenderMode = ToolStripRenderMode.System
        };

        ToolStripButton MkBtn(string text, string tip, Action click)
        {
            var b = new ToolStripButton(text) { ToolTipText = tip, ForeColor = Color.White };
            b.Click += (_, _) => click();
            return b;
        }

        toolbar.Items.Add(MkBtn("＋ New Profile", "Create a new profile", () => _profilesPage.NewProfile()));
        toolbar.Items.Add(new ToolStripSeparator());
        toolbar.Items.Add(MkBtn("💾 Save", "Save current profile", () => _profilesPage.SaveCurrent()));
        toolbar.Items.Add(MkBtn("✔ Activate", "Activate selected profile", () => _profilesPage.ActivateCurrent()));
        toolbar.Items.Add(new ToolStripSeparator());
        toolbar.Items.Add(MkBtn("📂 Open Cache Folder", "Open rendered page cache",
            () => OpenFolder(Core.SharedPaths.PageCacheDir)));
        toolbar.Items.Add(MkBtn("📋 Logs", "Open log folder",
            () => OpenFolder(Core.SharedPaths.LogDir)));

        Controls.Add(toolbar);
    }

    private void OnPageSelected(object? sender, string page) => ShowPage(page);

    private void ShowPage(string page)
    {
        Control target = page switch
        {
            "Profiles"    => _profilesPage,
            "Current Job" => _currentJobPage,
            "Behavior"    => _behaviorPage,
            "Diagnostics" => _diagnosticsPage,
            _             => _profilesPage
        };

        foreach (Control c in _content.Controls)
            c.Visible = false;

        target.Visible = true;

        if (target is IRefreshable r)
            r.Refresh();
    }

    private static void OpenFolder(string path)
    {
        Core.SharedPaths.EnsureDirectories();
        System.Diagnostics.Process.Start("explorer.exe", path);
    }
}

public interface IRefreshable
{
    void Refresh();
}
