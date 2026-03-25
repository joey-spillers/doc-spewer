using System.Runtime.InteropServices;
using OneView.VirtualScanner.Core;
using OneView.VirtualScanner.Core.Services;

namespace OneView.VirtualScanner.Manager.Pages;

public class DiagnosticsPage : UserControl, IRefreshable
{
    private readonly ScanLogger _logger = new();
    private readonly RichTextBox _log = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        BackColor = Color.FromArgb(20, 20, 26),
        ForeColor = Color.LimeGreen,
        Font = new Font("Consolas", 8.5f),
        ScrollBars = RichTextBoxScrollBars.Vertical
    };
    private readonly Label _archLabel = new() { AutoSize = false, Height = 22 };
    private readonly Label _x86Label  = new() { AutoSize = false, Height = 22 };
    private readonly Label _x64Label  = new() { AutoSize = false, Height = 22 };
    private readonly Label _cfgLabel  = new() { AutoSize = false, Height = 22 };

    private readonly Button _btnRefresh = new() { Text = "Refresh Logs",         Width = 130 };
    private readonly Button _btnOpenLog = new() { Text = "Open Log Folder",      Width = 130 };
    private readonly Button _btnInstall = new() { Text = "Install Source Files",  Width = 150 };
    private readonly Button _btnSelfTest = new() { Text = "Run Self-Test",        Width = 130 };

    public DiagnosticsPage()
    {
        BackColor = Color.FromArgb(245, 245, 247);
        BuildLayout();

        _btnRefresh.Click  += (_, _) => LoadLogs();
        _btnOpenLog.Click  += (_, _) => System.Diagnostics.Process.Start("explorer.exe", SharedPaths.LogDir);
        _btnInstall.Click  += (_, _) => InstallSourceFiles();
        _btnSelfTest.Click += (_, _) => SelfTest();

        LoadDiagnostics();
        LoadLogs();
    }

    public new void Refresh()
    {
        LoadDiagnostics();
        LoadLogs();
    }

    private void LoadDiagnostics()
    {
        _archLabel.Text = $"Process arch: {(IntPtr.Size == 8 ? "x64" : "x86")}";
        _x86Label.Text  = $"x86 DS path:  {SharedPaths.TwainSourceX86}  [{(Directory.Exists(SharedPaths.TwainSourceX86) ? "EXISTS" : "missing")}]";
        _x64Label.Text  = $"x64 DS path:  {SharedPaths.TwainSourceX64}  [{(Directory.Exists(SharedPaths.TwainSourceX64) ? "EXISTS" : "missing")}]";
        _cfgLabel.Text  = $"Config root:  {SharedPaths.ConfigRoot}  [{(Directory.Exists(SharedPaths.ConfigRoot) ? "OK" : "missing")}]";
    }

    private void LoadLogs()
    {
        var lines = _logger.GetRecent(400);
        _log.Text = string.Join(Environment.NewLine, lines);
        _log.SelectionStart = _log.Text.Length;
        _log.ScrollToCaret();
    }

    private void SelfTest()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"[{DateTime.Now:T}] Self-test starting...");

        // Check config dir
        sb.AppendLine(Directory.Exists(SharedPaths.ConfigRoot)
            ? "  ✓ Config directory exists"
            : "  ✗ Config directory MISSING");

        // Check active config
        if (File.Exists(SharedPaths.ActiveConfigFile))
        {
            var text = File.ReadAllText(SharedPaths.ActiveConfigFile);
            sb.AppendLine($"  ✓ active.json found: {text[..Math.Min(80, text.Length)]}");
        }
        else
            sb.AppendLine("  ✗ active.json not found — no profile activated yet");

        // Check PDF
        try
        {
            var pm = new OneView.VirtualScanner.Core.Services.ProfileManager();
            var active = pm.LoadActiveConfig();
            if (!string.IsNullOrEmpty(active.ActiveProfile))
            {
                var profiles = pm.LoadAll();
                var p = profiles.FirstOrDefault(x => x.Name == active.ActiveProfile);
                if (p != null)
                {
                    sb.AppendLine($"  ✓ Active profile: {p.Name}");
                    int pages = PdfRenderer.GetPageCount(p.PdfPath);
                    sb.AppendLine(pages > 0
                        ? $"  ✓ PDF readable: {pages} pages"
                        : $"  ✗ PDF not readable: {p.PdfPath}");
                }
            }
            else
                sb.AppendLine("  ⚠ No active profile set");
        }
        catch (Exception ex)
        {
            sb.AppendLine($"  ✗ Exception: {ex.Message}");
        }

        sb.AppendLine($"[{DateTime.Now:T}] Self-test complete.");
        _log.Text = sb.ToString();
    }

    private void InstallSourceFiles()
    {
        var appDir = AppContext.BaseDirectory;
        // Look for the .ds files next to the manager exe
        var x64Src = Path.Combine(appDir, "OneView.VirtualScanner.Source.dll");
        var installed = 0;

        foreach (var (src, dest, arch) in new[]
        {
            (x64Src, SharedPaths.TwainSourceX64, "x64")
        })
        {
            if (!File.Exists(src))
            {
                MessageBox.Show($"Source DLL not found: {src}\n\nBuild the Source project first.", "Install",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                continue;
            }

            try
            {
                Directory.CreateDirectory(dest);
                var destFile = Path.Combine(dest, "OneViewVS.ds");
                File.Copy(src, destFile, overwrite: true);
                _log.AppendText($"\r\nInstalled {arch} → {destFile}");
                installed++;
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show(
                    $"Cannot write to {dest}.\n\nRun the Manager as Administrator to install to the TWAIN folder.",
                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        if (installed > 0)
            MessageBox.Show($"Installed {installed} source file(s) into TWAIN directories.", "Done",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void BuildLayout()
    {
        var title = new Label
        {
            Text = "Diagnostics",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Dock = DockStyle.Top, Height = 36,
            Padding = new Padding(12, 4, 0, 0)
        };
        Controls.Add(title);

        var infoPanel = new Panel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(12, 4, 12, 4) };
        Controls.Add(infoPanel);

        foreach (var lbl in new[] { _archLabel, _x86Label, _x64Label, _cfgLabel })
        {
            lbl.Width = 700;
            lbl.Font = new Font("Consolas", 8.5f);
        }
        _archLabel.Top = 2;  _x86Label.Top = 24;
        _x64Label.Top  = 46; _cfgLabel.Top = 68;
        infoPanel.Controls.AddRange(new Control[] { _archLabel, _x86Label, _x64Label, _cfgLabel });

        var btnRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Top, Height = 40,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(8, 4, 0, 0)
        };
        foreach (var b in new[] { _btnSelfTest, _btnRefresh, _btnOpenLog, _btnInstall })
        {
            b.Height = 30;
            btnRow.Controls.Add(b);
        }
        Controls.Add(btnRow);

        var logLabel = new Label
        {
            Text = "Log Output",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Dock = DockStyle.Top, Height = 22,
            Padding = new Padding(8, 0, 0, 0)
        };
        Controls.Add(logLabel);
        Controls.Add(_log);
    }
}
