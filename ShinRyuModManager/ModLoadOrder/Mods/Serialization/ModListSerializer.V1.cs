using Utils;

namespace ShinRyuModManager.ModLoadOrder.Mods.Serialization;

// Original SRMM mod list format (ModList.txt)
public static partial class ModListSerializer
{
    private static List<ModInfo> ReadV1(string path)
    {
        var mods = new List<ModInfo>();
        
        var modContent = File.ReadAllText(path);
        
        if (string.IsNullOrWhiteSpace(modContent))
            return mods;
        
        foreach (var mod in modContent.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!mod.StartsWith('<') && !mod.StartsWith('>'))
                continue;
            
            var enabledMask = mod[0] == '<' ? ProfileMask.All : 0;
            var modName = mod[1..];
            
            if (!Directory.Exists(GamePath.GetModDirectory(modName)))
                continue;
            
            var entry = new ModInfo(modName, enabledMask);
            
            if (mods.Contains(entry))
                continue;
            
            mods.Add(entry);
        }
        
        return mods;
    }
}
