using Utils;

namespace ShinRyuModManager.ModLoadOrder.Mods.Serialization;

// Old mod list format (ModLoadOrder.txt)
public static partial class ModListSerializer
{
    private static List<ModInfo> ReadV0(string path)
    {
        var mods = new List<ModInfo>();
        
        foreach (var line in File.ReadLines(path))
        {
            if (line.StartsWith(';'))
                continue;
            
            var sanitizedLine = line.Split(';', 1, StringSplitOptions.TrimEntries)[0];
            
            if (string.IsNullOrEmpty(sanitizedLine) || !Directory.Exists(GamePath.GetModDirectory(sanitizedLine)))
                continue;
            
            var entry = new ModInfo(sanitizedLine);
            
            if (mods.Contains(entry))
                continue;
            
            mods.Add(entry);
        }
        
        return mods;
    }
}
