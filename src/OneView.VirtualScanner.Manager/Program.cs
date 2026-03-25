using OneView.VirtualScanner.Core;

namespace OneView.VirtualScanner.Manager;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        SharedPaths.EnsureDirectories();
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}