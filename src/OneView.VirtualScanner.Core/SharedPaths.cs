namespace OneView.VirtualScanner.Core;

public static class SharedPaths
{
    public static string ConfigRoot =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "OneView", "VirtualScanner");

    public static string ProfilesDir => Path.Combine(ConfigRoot, "profiles");
    public static string ActiveConfigFile => Path.Combine(ConfigRoot, "active.json");
    public static string BehaviorFile => Path.Combine(ConfigRoot, "behavior.json");
    public static string PageCacheDir => Path.Combine(ConfigRoot, "cache");
    public static string LogDir => Path.Combine(ConfigRoot, "logs");
    public static string TwainSourceX86 => @"C:\Windows\twain_32\OneView";
    public static string TwainSourceX64 => @"C:\Windows\twain_64\OneView";

    public static void EnsureDirectories()
    {
        Directory.CreateDirectory(ProfilesDir);
        Directory.CreateDirectory(PageCacheDir);
        Directory.CreateDirectory(LogDir);
    }
}
