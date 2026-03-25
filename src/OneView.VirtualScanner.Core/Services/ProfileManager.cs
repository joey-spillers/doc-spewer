using System.Text.Json;
using OneView.VirtualScanner.Core.Models;

namespace OneView.VirtualScanner.Core.Services;

public class ProfileManager
{
    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };

    public List<ScanProfile> LoadAll()
    {
        SharedPaths.EnsureDirectories();
        var profiles = new List<ScanProfile>();
        foreach (var f in Directory.GetFiles(SharedPaths.ProfilesDir, "*.json"))
        {
            try
            {
                var p = JsonSerializer.Deserialize<ScanProfile>(File.ReadAllText(f), JsonOpts);
                if (p != null) profiles.Add(p);
            }
            catch { /* skip corrupt files */ }
        }
        return profiles;
    }

    public void Save(ScanProfile profile)
    {
        SharedPaths.EnsureDirectories();
        var safe = MakeSafeName(profile.Name);
        var path = Path.Combine(SharedPaths.ProfilesDir, $"{safe}.json");
        File.WriteAllText(path, JsonSerializer.Serialize(profile, JsonOpts));
    }

    public void Delete(ScanProfile profile)
    {
        var safe = MakeSafeName(profile.Name);
        var path = Path.Combine(SharedPaths.ProfilesDir, $"{safe}.json");
        if (File.Exists(path)) File.Delete(path);
    }

    public void Rename(ScanProfile profile, string oldName)
    {
        var oldPath = Path.Combine(SharedPaths.ProfilesDir, $"{MakeSafeName(oldName)}.json");
        if (File.Exists(oldPath)) File.Delete(oldPath);
        Save(profile);
    }

    public ActiveConfig LoadActiveConfig()
    {
        SharedPaths.EnsureDirectories();
        if (!File.Exists(SharedPaths.ActiveConfigFile))
            return new ActiveConfig();
        try
        {
            return JsonSerializer.Deserialize<ActiveConfig>(
                File.ReadAllText(SharedPaths.ActiveConfigFile), JsonOpts) ?? new ActiveConfig();
        }
        catch { return new ActiveConfig(); }
    }

    public void SaveActiveConfig(ActiveConfig config)
    {
        SharedPaths.EnsureDirectories();
        File.WriteAllText(SharedPaths.ActiveConfigFile,
            JsonSerializer.Serialize(config, JsonOpts));
    }

    public BehaviorSettings LoadBehavior()
    {
        SharedPaths.EnsureDirectories();
        if (!File.Exists(SharedPaths.BehaviorFile))
            return new BehaviorSettings();
        try
        {
            return JsonSerializer.Deserialize<BehaviorSettings>(
                File.ReadAllText(SharedPaths.BehaviorFile), JsonOpts) ?? new BehaviorSettings();
        }
        catch { return new BehaviorSettings(); }
    }

    public void SaveBehavior(BehaviorSettings settings)
    {
        SharedPaths.EnsureDirectories();
        File.WriteAllText(SharedPaths.BehaviorFile,
            JsonSerializer.Serialize(settings, JsonOpts));
    }

    private static string MakeSafeName(string name) =>
        string.Concat(name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
}
