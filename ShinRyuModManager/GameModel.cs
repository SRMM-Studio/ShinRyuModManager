using ParLibrary;
using ParLibrary.Converter;
using Serilog;
using ShinRyuModManager.ModLoadOrder;
using Utils;
using Yarhl.FileSystem;

namespace ShinRyuModManager;

public static class GameModel
{
    public static bool SupportsUBIK(Game game)
    {
        return game >= Game.LostJudgment && game != Game.Eve;
    }
    
    // Yakuza 5 is quirky in the sense that accessing the loose files won't be enough
    // Things have to be duplicated in places like this
    // 1) Folder in root hact folder (hact/h1000_my_cool_new_hact)
    // 2) Par in root hact folder (hact/h1000_my_cool_new_hact.par)
    // 3) Folder + par in new hact folder (hact/h1000_my_cool_new_hact/000   AND   hact/h1000_my_cool_new_hact/000.par)
    // This can seriously bloat mod size. Instead of making modders duplicate their hacts like this
    // Let's do this for them (because we are nice)
    public static void DoY5HActProcedure(MLO mlo)
    {
        var hasHacts = false;
        var hactDirs = new HashSet<string>();
        
        foreach (var file in mlo.Files)
        {
            if (!file.Name.Contains("/hact/") || file.Name.EndsWith(".par"))
                continue;
            
            var mod = mlo.Mods[file.Index];
            var filePath = Path.Combine("mods", mod, file.Name.Trim('/'));
            var fileInfo = new FileInfo(filePath);
            // Get the folder from a path like so -> data/hact/h5000_some_hact/cmn/cmn.bin
            var hactDir = fileInfo.Directory!.Parent!.Name;
            
            if (fileInfo.Directory.Parent.Parent!.Name == "hact")
            {
                var hactPath = fileInfo.Directory.Parent.FullName;
                
                var parPath = Path.Combine(fileInfo.Directory.Parent.Parent.FullName, hactDir + ".par");
                
                // Legacy hact mods
                if (File.Exists(parPath))
                    continue;
                
                //Legacy hact mods
                if (!Directory.Exists(Path.Combine(hactPath, "000")) || !Directory.Exists(Path.Combine(hactPath, "cmn")))
                    continue;
                
                if (hactDirs.Contains(hactPath))
                    continue;
                
                if (GamePath.ExistsInDataAsPar(hactPath))
                    continue;
                
                hactDirs.Add(hactPath);
            }
            
            hasHacts = true;
        }
        
        if (!hasHacts)
            return;
        
        Log.Information("Repacking hacts for Yakuza 5..");
        
        foreach (var hactDirPath in hactDirs)
        {
            var hactDir = new DirectoryInfo(hactDirPath);
            var parlessDir = new DirectoryInfo(Path.Combine(GamePath.ParlessDir, "hact", hactDir.Name));
            
            if (!parlessDir.Exists)
                parlessDir.Create();
            
            foreach (var dir in hactDir.GetDirectories())
            {
                // We already repack ptc
                if (dir.Name == "ptc" && File.Exists(Path.Combine(hactDir.FullName, "ptc.par")))
                    continue;
                
                if (dir.Name != "ptc")
                {
                    var outputFakeDir = Path.Combine(parlessDir.FullName, dir.Name);
                    
                    Directory.CreateDirectory(outputFakeDir);
                    
                    var outputPath = Path.Combine(parlessDir.FullName, $"{dir.Name}.par");
                    Gibbed.Yakuza0.Pack.Program.Main([outputFakeDir], outputPath);
                    
                    try
                    {
                        new DirectoryInfo(outputFakeDir).Delete(true);
                    }
                    catch
                    {
                        // ignore
                    }
                }
                else
                {
                    var outputPath = Path.Combine(parlessDir.FullName, dir.Name + ".par");
                    Gibbed.Yakuza0.Pack.Program.Main([dir.FullName], outputPath);
                }
            }
            
            Gibbed.Yakuza0.Pack.Program.Main([parlessDir.FullName], Path.Combine(parlessDir.Parent!.FullName, $"{hactDir.Name}.par"));
        }
    }
    
    // One time upgrade of old mods
    public static void DoY0DCLegacyModsUpgrade(MLO mlo)
    {
        var parlessDir = new DirectoryInfo(GamePath.ParlessDir);
        
        parlessDir.Create();
        
        foreach (var modName in mlo.Mods)
        {
            var modDir = GamePath.GetModDirectory(modName);
            
            if (!Directory.Exists(modDir))
                continue;
            
            var legacyCharaDir = Path.Combine(modDir, "chara", "w64");
            var legacyStageDir = Path.Combine(modDir, "stage", "w64");
            var legacyReactorDir = Path.Combine(modDir, "reactorpar", "reactor_w64");
            
            if (Directory.Exists(legacyCharaDir))
            {
                var newCharaDir = Path.Combine(parlessDir.FullName, "chara", "ngen");
                
                for (var i = 0; i < mlo.Files.Count; i++)
                {
                    var file = mlo.Files[i];
                    
                    if (file.Name.Contains("chara/w64"))
                        file = file with
                        {
                            Name = file.Name.Replace("chara/w64", "chara/ngen")
                        };
                    
                    mlo.Files[i] = file;
                }
                
                Directory.Move(legacyCharaDir, newCharaDir);
            }
            
            if (Directory.Exists(legacyStageDir))
            {
                var newStageDir = Path.Combine(parlessDir.FullName, "stage", "ngen");
                
                for (var i = 0; i < mlo.Files.Count; i++)
                {
                    var file = mlo.Files[i];
                    
                    if (file.Name.Contains("stage/w64"))
                        file = file with
                        {
                            Name = file.Name.Replace("stage/w64", "stage/ngen")
                        };
                    
                    mlo.Files[i] = file;
                }
                
                Directory.Move(legacyStageDir, newStageDir);
            }
            
            if (Directory.Exists(legacyReactorDir))
            {
                var newReactorDir = Path.Combine(parlessDir.FullName, "reactorpar", "reactor_ngen");
                
                for (var i = 0; i < mlo.Files.Count; i++)
                {
                    var file = mlo.Files[i];
                    
                    if (file.Name.Contains("reactor_w64/"))
                        file = file with
                        {
                            Name = file.Name.Replace("reactor_w64/", "reactor_ngen/")
                        };
                    
                    mlo.Files[i] = file;
                }
                
                Directory.Move(legacyReactorDir, newReactorDir);
            }
        }
    }
    
    public static void DoYK2RemasterLegacyDBUpgrade(MLO mlo)
    {
        var parlessDir = new DirectoryInfo(GamePath.ParlessDir);
        
        parlessDir.Create();
        
        foreach (var modName in mlo.Mods)
        {
            var modDir = GamePath.GetModDirectory(modName);
            
            if (!Directory.Exists(modDir))
                continue;
            
            var legacyDBDir = Path.Combine(modDir, "db");
            var newDBDir = Path.Combine(modDir, "db.lexus2");
            
            var legacyPUIDDir = Path.Combine(modDir, "puid");
            var newPUIDDir = Path.Combine(modDir, "puid.lexus2");
            
            var haveOldDb = Directory.Exists(legacyDBDir);
            var haveOldPuid = Directory.Exists(legacyPUIDDir);
            
            if (haveOldDb || haveOldPuid)
            {
                for (var i = 0; i < mlo.Files.Count; i++)
                {
                    var file = mlo.Files[i];
                    
                    if (haveOldDb && file.Name.Contains("/db") && !file.Name.Contains("/db.lexus2"))
                    {
                        file = file with
                        {
                            Name = file.Name.Replace("/db", "/db.lexus2")
                        };
                    }
                    
                    if (haveOldPuid && file.Name.Contains("/puid") && !file.Name.Contains("/puid.lexus2"))
                    {
                        file = file with
                        {
                            Name = file.Name.Replace("/puid", "/puid.lexus2")
                        };
                    }
                    
                    mlo.Files[i] = file;
                }
            }
            
            if (haveOldDb)
                Directory.Move(legacyDBDir, newDBDir);
            
            if (haveOldPuid)
                Directory.Move(legacyPUIDDir, newPUIDDir);
        }
    }
    
    public static void DoOEHActProcedure(MLO mlo)
    {
        var hasHacts = false;
        var hactDirs = new HashSet<string>();
        
        foreach (var file in mlo.Files)
        {
            if (!file.Name.Contains("/hact/") || file.Name.EndsWith(".par"))
                continue;
            
            var mod = mlo.Mods[file.Index];
            var filePath = Path.Combine("mods", mod, file.Name.Trim('/'));
            var fileInfo = new FileInfo(filePath);
            
            //get the folder from a path like this -> data/hact/h5000_some_hact/cmn/cmn.bin
            var hactDir = fileInfo.Directory!.Parent!.Name;
            
            if (fileInfo.Directory.Parent.Parent!.Name == "hact")
            {
                var hactPath = fileInfo.Directory.Parent.FullName;
                
                var parPath = Path.Combine(fileInfo.Directory.Parent.Parent.FullName, hactDir + ".par");
                
                //Legacy hact mods
                if (File.Exists(parPath))
                    continue;
                
                //Legacy hact mods
                if (!Directory.Exists(Path.Combine(hactPath, "000")) || !Directory.Exists(Path.Combine(hactPath, "cmn")))
                    continue;
                
                if (!hactDirs.Add(hactPath))
                    continue;
            }
            
            hasHacts = true;
        }
        
        if (!hasHacts)
            return;
        
        Log.Information("Repacking hacts for Yakuza 0/Kiwami 1...");
        
        foreach (var hactDirPath in hactDirs)
        {
            var hactDir = new DirectoryInfo(hactDirPath);
            var parlessDir = new DirectoryInfo(Path.Combine(GamePath.ParlessDir, "hact", hactDir.Name));
            
            if (!parlessDir.Exists)
                parlessDir.Create();
            
            foreach (var dirName in hactDir.EnumerateDirectories().Select(x => x.Name))
            {
                //We already repack ptc 
                if (dirName == "ptc" /*&& File.Exists(Path.Combine(hactDir.FullName, "ptc.par"))*/)
                    continue;
                
                var outputFakeDir = Path.Combine(parlessDir.FullName, dirName);
                
                if (!Directory.Exists(outputFakeDir))
                    Directory.CreateDirectory(outputFakeDir);
                
                var outputPath = Path.Combine(parlessDir.FullName, dirName + ".par");
                
                Gibbed.Yakuza0.Pack.Program.Main([outputFakeDir], outputPath);
                
                try
                {
                    new DirectoryInfo(outputFakeDir).Delete(true);
                }
                catch
                {
                    // ignored
                }
            }
            
            Gibbed.Yakuza0.Pack.Program.Main([parlessDir.FullName], Path.Combine(parlessDir.Parent!.FullName, hactDir.Name + ".par"));
        }
    }
    
    public static void DoDEHActProcedure(MLO mlo, string codename)
    {
        var hasHacts = false;
        var hactDirs = new HashSet<string>();
        
        foreach (var file in mlo.Files)
        {
            if (!file.Name.Contains($"/hact_{codename}/") || file.Name.EndsWith(".par"))
                continue;
            
            var mod = mlo.Mods[file.Index];
            var filePath = Path.Combine("mods", mod, file.Name.Trim('/'));
            var fileInfo = new FileInfo(filePath);
            
            //get the folder from a path like this -> data/hact_yazawa/h5000_some_hact/cmn/cmn.bin
            var hactDir = fileInfo.Directory!.Parent!.Name;
            
            if (fileInfo.Directory.Parent.Parent!.Name == $"hact_{codename}")
            {
                var hactPath = fileInfo.Directory.Parent.FullName;
                
                var parPath = Path.Combine(fileInfo.Directory.Parent.Parent.FullName, hactDir + ".par");
                
                //Legacy hact mods
                if (File.Exists(parPath))
                    continue;
                
                //Legacy hact mods
                if (!Directory.Exists(Path.Combine(hactPath, "000")) || !Directory.Exists(Path.Combine(hactPath, "cmn")))
                    continue;
                
                if (hactDirs.Contains(hactPath))
                    continue;
                
                var isVanillaHact = GamePath.ExistsInDataAsPar(fileInfo.Directory.Parent.FullName);
                
                if (isVanillaHact)
                    continue;
                
                hactDirs.Add(hactPath);
            }
            
            hasHacts = true;
        }
        
        if (!hasHacts)
            return;
        
        foreach (var hactDirPath in hactDirs)
        {
            var hactDir = new DirectoryInfo(hactDirPath);
            var parlessDir = new DirectoryInfo(Path.Combine(GamePath.ParlessDir, "hact_" + codename, hactDir.Name));
            
            if (!parlessDir.Exists)
                parlessDir.Create();
            
            foreach (var dir in hactDir.GetDirectories())
            {
                //We already repack ptc 
                if (dir.Name == "ptc" && File.Exists(Path.Combine(hactDir.FullName, "ptc.par")))
                    continue;
                
                var outputPath = Path.Combine(parlessDir.FullName, dir.Name + ".par");
                Gibbed.Yakuza0.Pack.Program.Main([dir.FullName], outputPath);
            }
            
            Gibbed.Yakuza0.Pack.Program.Main([parlessDir.FullName], Path.Combine(parlessDir.Parent!.FullName, hactDir.Name + ".par"));
            
            new DirectoryInfo(parlessDir.FullName).Delete(true);
        }
    }
    
    // Got to be a better way to do this...
    public static void DoUBIKProcedure(MLO mlo)
    {
        var charaPath = Path.Combine("data/chara.par");
        
        if (!File.Exists(charaPath))
            return;
        
        var hasUbiks = mlo.Files.Any(file => file.Name.EndsWith(".ubik"));
        
        if (!hasUbiks)
            return;
        
        var ubikDir = Path.Combine(Constants.PARLESS_MODS_PATH, "ubik");
        var par = NodeFactory.FromFile(charaPath, "par");
        
        par.TransformWith<ParArchiveReader, ParArchiveReaderParameters>(new ParArchiveReaderParameters
        {
            Recursive = true
        });
        
        var ubik = Navigator.IterateNodes(par).FirstOrDefault(x => x.Path.EndsWith("ubik"));
        
        if (!Directory.Exists(Constants.PARLESS_MODS_PATH))
            Directory.CreateDirectory(Constants.PARLESS_MODS_PATH);
        
        if (!Directory.Exists(ubikDir))
            Directory.CreateDirectory(ubikDir);
        
        foreach (var node in ubik!.Children)
        {
            var ubikFile = node.GetFormatAs<ParFile>();
            
            if (ubikFile!.IsCompressed)
                node.TransformWith<ParLibrary.Sllz.Decompressor>();
            
            var filePath = Path.Combine(ubikDir, node.Name);
            
            if (node.Stream!.Length > 0)
                node.Stream.WriteTo(filePath);
        }
        
        foreach (var file in mlo.Files)
        {
            if (!file.Name.EndsWith(".ubik"))
                continue;
            
            var path = Path.Combine("mods", mlo.Mods[file.Index] + file.Name);
            File.Copy(path, Path.Combine(ubikDir, Path.GetFileName(file.Name)), true);
        }
        
        par.Dispose();
    }
    
    public static void DoYK3HActProcedure(MLO mlo, string codename)
    {
        var haveHAct = mlo.Files.Any(x => x.Name.Contains("/hact_"));
        
        if (!haveHAct)
            return;
        
        var hactDir = new DirectoryInfo(GamePath.DataPath)
                      .EnumerateDirectories()
                      .FirstOrDefault(x => x.Name.StartsWith("hact_"))?.FullName;
        
        if (string.IsNullOrEmpty(hactDir))
            return;
        
        var rootHActDir = Path.Combine(GamePath.ParlessDir, "hact_" + codename);
        
        Directory.CreateDirectory(rootHActDir);
        
        // This is really bad but it will have to do:
        // Get the smallest hact in the hact dir
        // Use that as a dummy file.
        // Our created pars inf load the game for some reason.
        var smallestHAct = new DirectoryInfo(hactDir)
                           .EnumerateFiles("*.par", SearchOption.TopDirectoryOnly)
                           .OrderBy(f => f.Length)
                           .First();
        
        foreach (var mod in mlo.Mods)
        {
            var modPath = GamePath.GetModDirectory(mod);
            var hActDirPath = Directory.EnumerateDirectories(modPath).FirstOrDefault(x => x.Contains("hact_"));
            
            if (string.IsNullOrEmpty(hActDirPath))
                continue;
            
            var hActDir = new DirectoryInfo(hActDirPath);
            
            foreach (var dir in hActDir.EnumerateDirectories())
            {
                var dummyParDir = new DirectoryInfo(rootHActDir);
                
                if (!dummyParDir.Exists)
                    dummyParDir.Create();
                
                var dummyParPath = new FileInfo(Path.Combine(dummyParDir.FullName, dir.Name + ".par"));
                File.Copy(smallestHAct.FullName, dummyParPath.FullName, true);
            }
        }
    }
    
    public static void DoTalkProcedureYK3(MLO mlo, string codename)
    {
        var haveTalk = mlo.Files.Any(x => x.Name.Contains("/talk_"));
        
        if (!haveTalk)
            return;
        
        var hactDir = new DirectoryInfo(GamePath.DataPath)
                      .EnumerateDirectories()
                      .FirstOrDefault(x => x.Name.StartsWith("hact_"))?.FullName;
        
        if (string.IsNullOrEmpty(hactDir))
            return;
        
        var rootTalkDir = Path.Combine(GamePath.ParlessDir, "talk_" + codename);
        
        if (!Directory.Exists(rootTalkDir))
            Directory.CreateDirectory(rootTalkDir);
        
        //This is really bad but it will have to do:
        //Get the smallest hact in the hact dir
        //Use that as a dummy file.
        //Our created pars inf load the game for some reason.
        
        foreach (var mod in mlo.Mods)
        {
            var modPath = GamePath.GetModDirectory(mod);
            var talkDirs = Directory.EnumerateDirectories(modPath).Where(x => x.Contains("talk_"));
            
            foreach (var modTalkDir in talkDirs)
            {
                var talksDirs = new DirectoryInfo(modTalkDir).EnumerateDirectories();
                
                foreach (var talkCategory in talksDirs)
                {
                    var dummyParDir = new DirectoryInfo(Path.Combine(rootTalkDir, talkCategory.Name));
                    
                    if (!dummyParDir.Exists)
                        dummyParDir.Create();
                    
                    foreach (var talkDir in talkCategory.EnumerateDirectories())
                    {
                        var talkPath = Path.Combine(dummyParDir.FullName, talkDir.Name);
                        
                        Directory.CreateDirectory(talkPath);
                        
                        if (!Directory.Exists(Path.Combine(talkDir.FullName, "000")) || !Directory.Exists(Path.Combine(talkDir.FullName, "cmn")))
                            continue;
                        
                        foreach (var dir in talkDir.EnumerateDirectories())
                        {
                            var outputPath = Path.Combine(talkPath, $"{dir.Name}.par");
                            
                            Utils.CreateParFromDirectory(dir.FullName, outputPath);
                        }
                        
                        Utils.CreateParFromDirectory(talkPath, $"{talkPath}.par");
                        
                        new DirectoryInfo(talkPath).Delete(true);
                    }
                }
            }
        }
    }
}
