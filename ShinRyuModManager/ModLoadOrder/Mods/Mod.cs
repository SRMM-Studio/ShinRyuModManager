using Serilog;
using Utils;

namespace ShinRyuModManager.ModLoadOrder.Mods;

public class Mod
{
    public string Name { get; }
    
    /// <summary>
    /// Files that can be directly loaded from the mod path.
    /// </summary>
    public List<string> Files { get; }
    
    /// <summary>
    /// Folders that have to be repacked into pars before running the game.
    /// </summary>
    public List<string> ParFolders { get; }
    
    /// <summary>
    /// Folders that need to be bound as a directory to a CPK binder.
    /// </summary>
    public List<string> CpkFolders { get; }
    
    /// <summary>
    /// Folders that need to be repacked.
    /// </summary>
    public List<string> RepackCpKs { get; }
    
    public Mod(ModInfo modInfo) : this(modInfo.Name) { }
    
    public Mod(string name)
    {
        Name = name;
        Files = [];
        ParFolders = [];
        CpkFolders = [];
        RepackCpKs = [];
        
        Log.Information("Reading directory: {Name} ...", name);
    }
    
    public void PrintInfo()
    {
        if (Files.Count > 0 || ParFolders.Count > 0)
        {
            if (Files.Count > 0)
            {
                Log.Information("Added {FilesCount} file(s)", Files.Count);
            }
            
            if (ParFolders.Count > 0)
            {
                Log.Information("Added {ParFoldersCount} folder(s) to be repacked", ParFolders.Count);
            }
            
            if (CpkFolders.Count > 0)
            {
                Log.Information("Added {CpkFoldersCount} CPK folder(s) to be bound", CpkFolders.Count);
            }
        }
        else
        {
            Log.Information("Nothing found for {Name}, skipping", Name);
        }
    }
    
    public void AddFiles(string path, string check)
    {
        var needsRepack = false;
        var basename = GamePath.GetBasename(path);
        var parentDir = new DirectoryInfo(path).Parent!.Name;
        
        // Check if this path does not need repacking
        if (Name != "Parless")
        {
            switch (check)
            {
                case "chara":
                case "map_":
                case "effect":
                    needsRepack = GamePath.ExistsInDataAsPar(path);
                    
                    break;
                case "prep":
                case "light_anim":
                    needsRepack = GamePath.CurrentGame < Game.Yakuza0 && GamePath.ExistsInDataAsPar(path);
                    
                    break;
                case "2d":
                case "cse":
                    needsRepack = (basename.StartsWith("sprite") || basename.StartsWith("pj")) &&
                        GamePath.ExistsInDataAsParNested(path);
                    
                    break;
                case "pausepar":
                    if (GamePath.CurrentGame >= Game.Yakuza0)
                        needsRepack = true;
                    else
                        needsRepack = !basename.StartsWith("pause") && GamePath.ExistsInDataAsPar(path);
                    
                    break;
                case "pausepar_e":
                    needsRepack = !basename.StartsWith("pause") && GamePath.ExistsInDataAsPar(path);
                    
                    break;
                case "particle":
                    if (GamePath.CurrentGame >= Game.Yakuza6 && basename == "arc")
                    {
                        check = "particle/arc";
                    }
                    
                    if (new DirectoryInfo(path).Parent?.Name == "arc_list")
                        needsRepack = true;
                    
                    break;
                case "particle/arc":
                    needsRepack = GamePath.ExistsInDataAsParNested(path);
                    
                    break;
                case "stage":
                    needsRepack = GamePath.CurrentGame == Game.Eve && basename == "sct" &&
                        GamePath.ExistsInDataAsParNested(path);
                    
                    break;
                case "":
                    needsRepack = (basename == "ptc" && GamePath.ExistsInDataAsParNested(path))
                        || (basename == "entity_adam" && GamePath.ExistsInDataAsPar(path));
                    
                    if (!needsRepack)
                    {
                        check = CheckFolder(basename);
                    }
                    
                    break;
            }
            
            // Check for CPK directories
            string cpkDataPath;
            
            switch (basename)
            {
                case "bgm":
                    if (GamePath.CurrentGame <= Game.YakuzaKiwami_R)
                    {
                        cpkDataPath = GamePath.RemoveModPath(path);
                        RepackCpKs.Add(cpkDataPath);
                    }
                    
                    break;
                
                case "se":
                case "speech":
                    if (GamePath.CurrentGame is Game.Yakuza5 or <= Game.YakuzaKiwami_R)
                    {
                        // Removed adding ".cpk" to se folder. Seemed incorrect
                        cpkDataPath = GamePath.RemoveModPath(path);
                        RepackCpKs.Add(cpkDataPath);
                    }
                    
                    break;
                case "stream":
                case "stream_en":
                case "stmdlc":
                case "stmdlc_en":
                case "movie":
                case "moviesd":
                case "moviesd_dlc":
                    cpkDataPath = GamePath.RemoveModPath(path);
                    
                    if (GamePath.CurrentGame is Game.Judgment or Game.LostJudgment)
                    {
                        CpkFolders.Add(cpkDataPath + ".par");
                        Log.Verbose("Adding CPK folder: {CpkDataPath}", cpkDataPath);
                    }
                    
                    break;
                case "gv_files":
                    cpkDataPath = GamePath.RemoveModPath(path);
                    
                    CpkFolders.Add($"{cpkDataPath}.cpk");
                    Log.Verbose("Adding CPK folder: {CpkDataPath}", cpkDataPath);
                    
                    break;
            }
            
            if (parentDir != basename)
            {
                switch (parentDir)
                {
                    case "motion":
                        //if (game == Game.Yakuza5)
                        // needsRepack = GamePath.ExistsInDataAsPar(path) && basename.ToLowerInvariant().Contains("Battle");
                        break;
                }
            }
            
            if (GamePath.CurrentGame >= Game.Yakuza6 && GamePath.CurrentGame != Game.YakuzaKiwami3)
            {
                //Dragon Engine talks use pars directly for these
                if (path.Contains("talk_"))
                {
                    if (char.IsDigit(basename[0]) || check == "cmn")
                    {
                        needsRepack = true;
                    }
                    else
                    {
                        var tCmn = Path.Combine(path, "cmn");
                        var t000 = Path.Combine(path, "000");
                        
                        if (Directory.Exists(tCmn) && Directory.Exists(t000))
                            needsRepack = true;
                    }
                }
            }
            
            if (GamePath.CurrentGame >= Game.LikeADragonPirates)
            {
                // Additional game specific checks
                switch (basename)
                {
                    // Pirates in Hawaii stores gmts inside folders based on the lowercase filename checksum.
                    // For the modder's convenience, move any gmts in the folder root to the corresponding subdirectory.
                    case "motion":
                    {
                        var gmtFolderPath = Path.Combine(path, "gmt");
                        
                        if (!Directory.Exists(gmtFolderPath))
                            break;
                        
                        var baseParlessPath = Path.Combine(GamePath.ParlessDir, "motion", "gmt");
                        
                        foreach (var p in Directory.EnumerateFiles(gmtFolderPath).Where(f => !f.EndsWith(Constants.VORTEX_MANAGED_FILE)).Select(GamePath.GetDataPathFrom))
                        {
                            // Copy any gmts to the appropriate hash folder in Parless
                            if (!p.EndsWith(".gmt", StringComparison.InvariantCultureIgnoreCase))
                                continue;
                            
                            var fileName = Path.GetFileName(p);
                            var fileNameNoExt = Path.GetFileNameWithoutExtension(p).ToLowerInvariant();
                            
                            var hash = new PXDHash();
                            hash.Set(fileNameNoExt);
                            
                            var gmtPath = Path.Combine(gmtFolderPath, fileName);
                            
                            var checksum = string.Concat("00", hash.Checksum.ToString("x4").AsSpan(2, 2));
                            var destinationDirectory = Path.Combine(baseParlessPath, checksum);
                            
                            if (!Directory.Exists(destinationDirectory))
                                Directory.CreateDirectory(destinationDirectory);
                            
                            File.Copy(gmtPath, Path.Combine(destinationDirectory, Path.GetFileName(gmtPath)));
                        }
                        
                        break;
                    }
                }
            }
        }
        
        if (needsRepack)
        {
            var dataPath = GamePath.GetDataPathFrom(path);
            
            // Add this folder to the list of folders to be repacked and stop recursing
            ParFolders.Add(dataPath);
            Log.Verbose("Adding repackable folder: {DataPath}", dataPath);
        }
        else
        {
            // Add files in current directory
            var files = Directory.GetFiles(path).Where(f => !f.EndsWith(Constants.VORTEX_MANAGED_FILE)).Select(GamePath.GetDataPathFrom);
            
            foreach (var file in files)
            {
                // No need to add the meta file
                if (file.EndsWith("mod-meta.yaml"))
                    continue;
                
                Files.Add(file);
                Log.Verbose("Adding file: {file}", file);
            }
            
            var isParlessMod = GetType() == typeof(ParlessMod);
            
            // Get files for all subdirectories
            foreach (var folder in Directory.GetDirectories(path))
            {
                // Break an important rule in the concept of inheritance to make the program function correctly
                if (isParlessMod)
                {
                    ((ParlessMod)this).AddFiles(folder, check);
                }
                else
                {
                    AddFiles(folder, check);
                }
            }
        }
    }
    
    protected static string CheckFolder(string name)
    {
        var folder = Constants.IncompatiblePars.FirstOrDefault(name.StartsWith);
        
        return folder ?? string.Empty;
    }
}
