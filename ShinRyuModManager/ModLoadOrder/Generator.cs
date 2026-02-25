using Serilog;
using Serilog.Events;
using ShinRyuModManager.ModLoadOrder.Mods;
using Utils;

namespace ShinRyuModManager.ModLoadOrder;

public static class Generator
{
    public static async Task<MLO> GenerateModeLoadOrder(List<ModInfo> mods, bool looseFilesEnabled, bool cpkRepackingEnabled)
    {
        List<int> modIndices = [0];
        var files = new OrderedSet<string>();
        var modsWithFoldersNotFound = new Dictionary<string, List<string>>(); // Dict of Mod, ListOfFolders
        var parDictionary = new Dictionary<string, List<string>>();           // Dict of PathToPar, ListOfMods
        
        var loose = new ParlessMod();
        
        if (looseFilesEnabled)
        {
            loose.AddFiles(GamePath.DataPath, "");
            loose.PrintInfo();
            
            // Add all pars to the dictionary
            foreach (var par in loose.ParFolders)
            {
                var index = par.IndexOf(".parless", StringComparison.Ordinal);
                
                if (index != -1)
                {
                    // Remove .parless from the par's path
                    // Since .parless loose files are processed first, we can be sure that the dictionary won't have duplicates
                    parDictionary.Add(par.Remove(index, 8), [$"{loose.Name}_{index}"]);
                }
            }
            
            Log.Information($"Done reading {Constants.PARLESS_NAME}\n");
        }
        
        var modsObjects = new Mod[mods.Count];
        var cpkDictionary = new Dictionary<string, List<int>>();
        
        Log.Information("Reading mods...\n");
        
        // TODO: Make mod reading async
        
        // Use a reverse loop to be able to remove items from the list when necessary
        for (var i = mods.Count - 1; i >= 0; i--)
        {
            var mod = new Mod(mods[i]);
            var modPath = GamePath.GetModDirectory(mod.Name);
            
            mod.AddFiles(modPath, "");
            
            mod.PrintInfo();
            
            if (mod.Files.Count > 0 || mod.ParFolders.Count > 0 || mod.CpkFolders.Count > 0)
            {
                files.UnionWith(mod.Files);
                modIndices.Add(files.Count);
                
                foreach (var folder in mod.CpkFolders)
                {
                    if (!cpkDictionary.TryGetValue(folder, out var value))
                    {
                        value = [];
                        cpkDictionary[folder] = value;
                    }
                    
                    value.Add(mods.Count - 1 - i);
                }
            }
            else
            {
                mods.RemoveAt(i);
            }
            
            // Add all pars to the dictionary
            foreach (var par in mod.ParFolders)
            {
                if (parDictionary.TryGetValue(par, out var list))
                {
                    // Add the mod's name to the par's list
                    list.Add(mod.Name);
                }
                else
                {
                    // If a par is not in the dictionary, make a new list for it
                    parDictionary.Add(par, [mod.Name]);
                }
            }
            
            // Check for folders which do not exist in the data path in the mod's root
            List<string> foldersNotFound = [];
            
            foreach (var subPath in Directory.EnumerateDirectories(modPath))
            {
                var subPathName = new DirectoryInfo(subPath).Name;
                
                if (GamePath.DirectoryExistsInData(subPathName) || GamePath.FileExistsInData($"{subPathName}.par"))
                    continue;
                
                // While "stream" isn't a folder in Y0 or Kiwami, it shouldn't warn the user as it's used in place of bgm.cpk
                if (GamePath.CurrentGame is Game.Yakuza0 or Game.YakuzaKiwami && subPathName == "stream")
                    continue;
                
                foldersNotFound.Add(subPathName);
            }
            
            if (foldersNotFound.Count != 0)
            {
                modsWithFoldersNotFound.Add(mod.Name, foldersNotFound);
            }
            
            modsObjects[i] = mod;
        }
        
        Log.Information("Added {ModCount} mod(s) and {FilesCount} file(s)!", mods.Count, files.Count);
        
        // Reverse the list because the last mod in the list should have the highest priority
        mods.Reverse();
        
        Log.Information($"Generating {Constants.MLO} file...");
        
        var modNames = mods.Select(x => x.Name).ToList();
        
        // Generate MLO
        var mlo = new MLO(modIndices, modNames, files, loose.ParlessFolders, cpkDictionary);
        
        mlo.WriteMLO(Path.Combine(GamePath.FullGamePath, Constants.MLO));
        
        Log.Information("Finished generating MLO.");
        
        // Check if a mod has a par that will override the repacked par, and skip repacking it in that case
        foreach (var key in parDictionary.Keys.ToList())
        {
            var value = new ModInfo(parDictionary[key][0], 0);
            
            // Faster lookup by checking in the OrderedSet
            if (!files.Contains($"{key}.par"))
                continue;
            
            // Get the mod's index from the ModLoadOrder's Files
            var matchIndex = mlo.Files.Find(f => f.Name == Utils.NormalizeToNodePath($"{key}.par")).Index;
            
            // Avoid repacking pars which exist as a file in mods that have a higher priority that the first mod in the par to be repacked
            if (mods.IndexOf(value) > matchIndex)
            {
                parDictionary.Remove(key);
            }
        }
        
        var cpkRepackDict = new Dictionary<string, List<string>>();
        
        foreach (var modObj in modsObjects)
        {
            foreach (var str in modObj.RepackCpKs)
            {
                if (!cpkDictionary.ContainsKey(str))
                {
                    cpkRepackDict.Add(str, []);
                }
                
                cpkRepackDict[str].Add(modObj.Name);
            }
        }
        
        await ParRepacker.RepackDictionary(parDictionary);
        
        if (cpkRepackingEnabled)
        {
            await CpkPatcher.RepackDictionary(cpkRepackDict);
        }
        
        if (Program.LogLevel > LogEventLevel.Warning)
            return mlo;
        
        foreach (var key in modsWithFoldersNotFound.Keys.ToList())
        {
            Log.Warning("Warning: Some folders in the root of \"{Key}\" do not exist in the game's data. Check if the mod was extracted correctly.", key);
            
            if (Program.LogLevel != LogEventLevel.Verbose)
                continue;
            
            foreach (var folder in modsWithFoldersNotFound[key])
            {
                Log.Warning("Folder not found: {Folder}", folder);
            }
        }
        
        return mlo;
    }
}
