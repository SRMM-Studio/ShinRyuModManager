using System.Diagnostics;
using System.IO.Compression;
using Avalonia;
using Avalonia.Svg.Skia;
using CpkTools.Model;
using IniParser;
using IniParser.Model;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Json;
using ShinRyuModManager.Extensions;
using ShinRyuModManager.Helpers;
using ShinRyuModManager.ModLoadOrder;
using ShinRyuModManager.ModLoadOrder.Mods;
using ShinRyuModManager.ModLoadOrder.Mods.Serialization;
using ShinRyuModManager.Templates;
using ShinRyuModManager.UserInterface;
using Utils;
using Constants = Utils.Constants;

namespace ShinRyuModManager;

public static class Program
{
    private static bool _externalModsOnly = true;
    private static bool _looseFilesEnabled;
    private static bool _cpkRepackingEnabled = true;
    private static bool _checkForUpdates = true;
    private static bool _isSilent;
    private static IniData _iniData;
    
    private static readonly FileIniDataParser IniParser = new()
    {
        Parser =
        {
            Configuration =
            {
                AssigmentSpacer = string.Empty
            }
        }
    };
    
    // Mod List file information
    public static byte CurrentModListVersion { get => 2; }
    
    public static Profile ActiveProfile { get; set; } = Profile.Profile1;
    
    public static bool RebuildMlo { get; private set; } = true;
    public static bool IsRebuildMloSupported { get; private set; } = true;
    public static LogEventLevel LogLevel { get; private set; } = LogEventLevel.Information;
    public static List<LibMeta> LibraryMetaCache { get; set; } = [];
    
    [STAThread]
    private static void Main(string[] args)
    {
        Directory.CreateDirectory(Constants.LOGS_BASE_PATH);
        
        var defaultLogsPath = Path.Combine(Constants.LOGS_BASE_PATH, "srmm_logs.log");
        var errorLogsPath = Path.Combine(Constants.LOGS_BASE_PATH, "srmm_errors.log");
        
        LoadConfig();
        
        // Create global logger
        Log.Logger = new LoggerConfiguration()
                     .MinimumLevel.ControlledBy(new LoggingLevelSwitch(LogLevel))
                     .Enrich.WithExceptionDetails()
                     // Log to SRMM logs file
                     .WriteTo.Logger(l => l
                                          .Filter.ByIncludingOnly(e => e.Exception == null)
                                          .WriteTo.Async(a => a.File(defaultLogsPath, rollingInterval: RollingInterval.Day))
                                          .WriteTo.Async(a => a.Console()))
                     // Logs exceptions separately
                     .WriteTo.Logger(l => l
                                          .Filter.ByIncludingOnly(e => e.Exception != null)
                                          .WriteTo.Async(a => a.File(new JsonFormatter(renderMessage: true), errorLogsPath, rollingInterval: RollingInterval.Day)))
                     .CreateLogger();
        
        if (!OperatingSystem.IsWindows())
        {
            IsRebuildMloSupported = false;
        }
        
        // Check if there are any args, if so, run in CLI mode
        // Unfortunately, no one way to detect left Ctrl while being cross-platform
        if (args.Length == 0)
        {
            Log.Information("Shin Ryu Mod Manager GUI Application Start");
            
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        else
        {
            Log.Information("Shin Ryu Mod Manager CLI Mode Start");
            
            MainCli(args).GetAwaiter().GetResult();
        }
    }
    
    private static AppBuilder BuildAvaloniaApp()
    {
        GC.KeepAlive(typeof(SvgImageExtension).Assembly);
        GC.KeepAlive(typeof(Svg.Skia.SKSvg).Assembly);
        
        return AppBuilder.Configure<App>()
                         .UsePlatformDetect()
                         .WithInterFont()
                         .LogToTrace();
    }
    
    private static async Task MainCli(string[] args)
    {
        Log.Information("Shin Ryu Mod Manager v{Version}", AssemblyVersion.GetVersion());
        
        // Parse arguments
        var list = new List<string>(args);
        
        if (list.Contains("-h") || list.Contains("--help"))
        {
            Log.Information("""
                            Usage: run without arguments to generate mod load order.
                              -s, --silent     prevent checking for updates and remove prompts.
                              -h, --help       show this message and exit.
                              -r, --run        run the game after the program finishes.
                            """);
            
            return;
        }
        
        if (list.Contains("-s") || list.Contains("--silent"))
        {
            _isSilent = true;
        }
        
        await RunGeneration(PreRun().Where(x => x.Enabled).ToList());
        
        PostRun();
        
        await Log.CloseAndFlushAsync();
        
        if (list.Contains("-r") || list.Contains("--run"))
        {
            var gameLaunchPath = GamePath.GetGameLaunchPath();
            
            if (!string.IsNullOrEmpty(gameLaunchPath))
            {
                if (OperatingSystem.IsWindows())
                {
                    Process.Start(new ProcessStartInfo(gameLaunchPath)
                    {
                        UseShellExecute = true
                    });
                }
                else if (OperatingSystem.IsLinux())
                {
                    Process.Start("xdg-open", gameLaunchPath);
                }
            }
            else
            {
                Log.Error("The game can't be launched. Please launch manually.");
            }
        }
    }
    
    private static void LoadConfig()
    {
        if (File.Exists(Constants.INI))
        {
            _iniData = IniParser.ReadFile(Constants.INI);
            
            if (_iniData.TryGetKey("Overrides.LooseFilesEnabled", out var looseFiles))
            {
                _looseFilesEnabled = int.Parse(looseFiles) == 1;
            }
            
            if (_iniData.TryGetKey("RyuModManager.Verbose", out var verbose) && int.Parse(verbose) == 1)
            {
                LogLevel = LogEventLevel.Verbose;
            }
            
            if (_iniData.TryGetKey("RyuModManager.CheckForUpdates", out var check))
            {
                _checkForUpdates = int.Parse(check) == 1;
            }
            
            if (_iniData.TryGetKey("RyuModManager.ShowWarnings", out var showWarnings))
            {
                //ConsoleOutput.ShowWarnings = int.Parse(showWarnings) == 1;
            }
            
            if (_iniData.TryGetKey("RyuModManager.LoadExternalModsOnly", out var extMods))
            {
                _externalModsOnly = int.Parse(extMods) == 1;
            }
            
            if (_iniData.TryGetKey("Overrides.RebuildMLO", out var rebuildMlo))
            {
                RebuildMlo = int.Parse(rebuildMlo) == 1;
            }
            
            if (!_iniData.TryGetKey("Parless.IniVersion", out var iniVersion) ||
                int.Parse(iniVersion) < ParlessIni.CURRENT_VERSION)
            {
                // Update if ini version is old (or does not exist)
                Log.Information($"{Constants.INI} is outdated. Updating ini to the latest version... ");
                
                if (int.Parse(iniVersion) <= 3)
                {
                    // Force enable RebuildMLO option
                    _iniData.Sections["Overrides"]["RebuildMLO"] = "1";
                    RebuildMlo = true;
                }
                
                IniParser.WriteFile(Constants.INI, IniTemplate.UpdateIni(_iniData));
                Log.Information($"Updated {Constants.INI}");
            }
        }
        else
        {
            // Create ini if it does not exist
            Log.Information($"{Constants.INI} was not found. Creating default ini...");
            IniParser.WriteFile(Constants.INI, IniTemplate.NewIni());
        }
    }
    
    internal static List<ModInfo> PreRun(Profile? profile = null)
    {
        if (GamePath.CurrentGame != Game.Unsupported)
        {
            Directory.CreateDirectory(GamePath.MODS);
            Directory.CreateDirectory(GamePath.LIBRARIES);
        }
        
        Log.Information("Active Profile: {Profile}", profile?.GetDescription() ?? ActiveProfile.GetDescription());
        
        if (File.Exists(Constants.DINPUT8DLL) && File.Exists(Constants.WINMMLJ))
        {
            // TODO: Maybe move this to a separate "Game patches" file
            // Virtua Fighter eSports crashes when used with dinput8.dll as the ASI loader
            if (GamePath.CurrentGame == Game.Eve && File.Exists(Constants.DINPUT8DLL))
            {
                if (File.Exists(Constants.VERSIONDLL))
                {
                    Log.Warning($"Game specific patch: Deleting {Constants.DINPUT8DLL} because {Constants.VERSIONDLL} exists...");
                    
                    // Remove dinput8.dll
                    File.Delete(Constants.DINPUT8DLL);
                }
                else
                {
                    Log.Warning($"Game specific patch: Renaming {Constants.DINPUT8DLL} to {Constants.VERSIONDLL}...");
                    
                    // Rename dinput8.dll to version.dll to prevent the game from crashing
                    File.Move(Constants.DINPUT8DLL, Constants.VERSIONDLL);
                }
            }
            else if (GamePath.CurrentGame is >= Game.YakuzaKiwami2 and not Game.LikeADragonGaiden)
            {
                // Lost Judgment (and Judgment post update 1) does not like Ultimate ASI Loader, so instead we use a custom build of DllSpoofer (https://github.com/Kazurin-775/DllSpoofer)
                if (File.Exists(Constants.DINPUT8DLL))
                {
                    Log.Warning($"Game specific patch: Deleting {Constants.DINPUT8DLL} because it causes crashes with Judgment games...");
                    
                    // Remove dinput8.dll
                    File.Delete(Constants.DINPUT8DLL);
                }
                
                if (!File.Exists(Constants.WINMMDLL))
                {
                    if (File.Exists(Constants.WINMMLJ))
                    {
                        Log.Warning($"Game specific patch: Enabling {Constants.WINMMDLL} by renaming {Constants.WINMMLJ} to fix Judgment games crashes...");
                        
                        // Rename dinput8.dll to version.dll to prevent the game from crashing
                        File.Move(Constants.WINMMLJ, Constants.WINMMDLL);
                    }
                    else
                    {
                        Log.Error($"WARNING: {Constants.WINMMLJ} was not found. Judgment games will NOT load mods without this file. Please redownload Shin Ryu Mod Manager.");
                    }
                }
            }
        }
        
        // Read ini (again) to check if we should try importing the old load order file
        _iniData = IniParser.ReadFile(Constants.INI);
        
        if (GamePath.CurrentGame is Game.Judgment or Game.LostJudgment or Game.LikeADragonPirates
            && _iniData.TryGetKey("Overrides.RebuildMLO", out _))
        {
            // Disable RebuildMLO when using an external mod manager
            Log.Warning("Game specific patch: Disabling RebuildMLO for some games when using an external mod manager...");
            
            _iniData.Sections["Overrides"]["RebuildMLO"] = "0";
            IniParser.WriteFile(Constants.INI, _iniData);
            RebuildMlo = false;
        }
        
        var mods = new List<ModInfo>();
        
        if (ShouldBeExternalOnly())
        {
            // Only load the files inside the external mods path, and ignore the load order in the txt
            mods.Add(new ModInfo(Constants.EXTERNAL_MODS, 0));
        }
        else
        {
            if (File.Exists(Constants.TXT_OLD))
            {
                mods.AddRange(ModListSerializer.Read(Constants.TXT_OLD, profile));
                
                File.Move(Constants.TXT_OLD, $"{Constants.TXT_OLD}.bak", true);
            }
            else if (File.Exists(Constants.TXT))
            {
                mods.AddRange(ModListSerializer.Read(Constants.TXT, profile));
                
                File.Move(Constants.TXT, $"{Constants.TXT}.bak", true);
            }
            else if (File.Exists(Constants.MOD_LIST))
            {
                mods.AddRange(ModListSerializer.Read(Constants.MOD_LIST, profile));
            }
            else
            {
                Log.Information($"{Constants.MOD_LIST} was not found. Will load all existing mods.\n");
            }
            
            if (Directory.Exists(GamePath.ModsPath))
            {
                // Add all scanned mods that have not been added to the load order yet
                Log.Information("Scanning for mods...");
                
                foreach (var newMod in ScanMods().Where(newMod => !mods.Contains(newMod)))
                {
                    mods.Add(newMod);
                }
                
                Log.Information("Found {ModsCount} mods.", mods.Count);
            }
            
            Log.Information("Enabled Mods:");
            
            foreach (var mod in mods.Where(x => x.Enabled))
            {
                Log.Information("    {ModName}", mod.Name);
            }
        }
        
        if (!GamePath.IsXbox(Path.Combine(GamePath.FullGamePath)) || !_iniData.TryGetKey("Overrides.RebuildMLO", out _))
            return mods;
        
        Log.Warning("Game specific patch: Disabling RebuildMLO for Xbox games...");
        
        _iniData.Sections["Overrides"]["RebuildMLO"] = "0";
        IniParser.WriteFile(Constants.INI, _iniData);
        RebuildMlo = false;
        
        return mods;
    }
    
    internal static async Task RunGeneration(List<ModInfo> mods)
    {
        if (File.Exists(Constants.MLO))
        {
            Log.Information("Removing old MLO...");
            
            // Remove existing MLO file to avoid it being used if a new MLO won't be generated
            File.Delete(Constants.MLO);
        }
        
        // Remove previously repacked pars, to avoid unwanted side effects
        ParRepacker.RemoveOldRepackedPars();
        
        if (GamePath.CurrentGame == Game.Unsupported)
        {
            Log.Warning("Aborting: No supported game was found in this directory");
            
            return;
        }
        
        if (!mods.IsNullOrEmpty() || _looseFilesEnabled)
        {
            // Create Parless mod as highest priority
            var parlessEntry = mods.FirstOrDefault(x => string.Equals(x.Name, "Parless", StringComparison.InvariantCultureIgnoreCase));
            
            if (parlessEntry != null)
            {
                mods.Remove(parlessEntry);
                mods.Insert(0, parlessEntry);
            }
            else
            {
                mods.Insert(0, new ModInfo("Parless", 0));
            }
            
            Directory.CreateDirectory(Constants.PARLESS_MODS_PATH);
            
            Log.Information("Generating MLO...");
            
            var sw = Stopwatch.StartNew();
            
            var result = await Generator.GenerateModeLoadOrder(mods, _looseFilesEnabled, _cpkRepackingEnabled);
            
            if (GameModel.SupportsUBIK(GamePath.CurrentGame))
            {
                GameModel.DoUBIKProcedure(result);
            }
            
            switch (GamePath.CurrentGame)
            {
                case Game.Yakuza5:
                    GameModel.DoY5HActProcedure(result);
                    
                    break;
                
                case Game.Yakuza0:
                case Game.YakuzaKiwami:
                    GameModel.DoOEHActProcedure(result);
                    
                    break;
                
                case Game.Yakuza0_DC:
                case Game.YakuzaKiwami_R:
                    GameModel.DoOEHActProcedure(result);
                    GameModel.DoY0DCLegacyModsUpgrade(result);
                    
                    break;
                
                case Game.YakuzaKiwami2:
                    GameModel.DoDEHActProcedure(result, "lexus2");
                    
                    break;
                
                case Game.YakuzaKiwami2_R:
                    GameModel.DoDEHActProcedure(result, "lexus2");
                    GameModel.DoYK2RemasterLegacyDBUpgrade(result);
                    
                    break;
                
                case Game.Judgment:
                    GameModel.DoDEHActProcedure(result, "judge");
                    
                    break;
                
                case Game.YakuzaLikeADragon:
                    GameModel.DoDEHActProcedure(result, "yazawa");
                    
                    break;
                
                case Game.LikeADragonGaiden:
                    GameModel.DoDEHActProcedure(result, "aston");
                    
                    break;
                
                case Game.LostJudgment:
                    GameModel.DoDEHActProcedure(result, "coyote");
                    
                    break;
                
                case Game.LikeADragon8:
                    GameModel.DoDEHActProcedure(result, "elvis");
                    
                    break;
                
                case Game.LikeADragonPirates:
                    GameModel.DoDEHActProcedure(result, "spr");
                    
                    break;
                
                case Game.YakuzaKiwami3:
                    GameModel.DoYK3HActProcedure(result, "bis");
                    GameModel.DoTalkProcedureYK3(result, "bis");
                    
                    break;
            }
            
            sw.Stop();
            Log.Information("MLO Generation took: {ElapsedTotalSeconds} seconds", sw.Elapsed.TotalSeconds);
        }
    }
    
    private static void PostRun()
    {
        // Check if the ASI loader is not in the directory (possibly due to incorrect zip extraction)
        if (MissingDll())
        {
            Log.Warning($"Warning: \"{Constants.DINPUT8DLL}\" is missing from this directory. Shin Ryu Mod Manager will NOT function properly without this file");
        }
        
        // Check if the ASI is not in the directory
        if (MissingAsi())
        {
            Log.Warning($"Warning: \"{Constants.ASI}\" is missing from this directory. Shin Ryu Mod Manager will NOT function properly without this file");
        }
        
        if (!_isSilent)
        {
            Log.Information("Program finished. Press any key to exit...");
            Console.ReadKey();
        }
    }
    
    internal static bool ShouldBeExternalOnly()
    {
        return _externalModsOnly && Directory.Exists(GamePath.ExternalModsPath);
    }
    
    private static List<ModInfo> ScanMods(ProfileMask activeProfiles = ProfileMask.All)
    {
        var mods = new List<ModInfo>();
        
        foreach (var dir in Directory.EnumerateDirectories(GamePath.ModsPath))
        {
            var modPath = Path.GetFileName(dir.TrimEnd(Path.DirectorySeparatorChar));
            
            if (string.Equals(modPath, "Parless") || string.Equals(modPath, Constants.EXTERNAL_MODS))
                continue;
            
            var entry = new ModInfo(modPath, activeProfiles);
            
            if (mods.Contains(entry))
                continue;
            
            mods.Add(entry);
        }
        
        return mods;
    }
    
    internal static bool MissingDll()
    {
        return !(File.Exists(Constants.DINPUT8DLL) || File.Exists(Constants.VERSIONDLL) || File.Exists(Constants.WINMMDLL));
    }
    
    internal static bool MissingAsi()
    {
        return !File.Exists(Constants.ASI);
    }
    
    internal static void SaveModList(List<ModInfo> mods)
    {
        ModListSerializer.Write(Constants.MOD_LIST, mods);
    }
    
    public static string[] GetModDependencies(string mod)
    {
        var modDir = GamePath.GetModDirectory(mod);
        var metaFile = Path.Combine(modDir, "mod-meta.yaml");
        
        if (!Directory.Exists(modDir) || !File.Exists(metaFile))
            return [];
        
        var meta = YamlHelpers.DeserializeYamlFromPath<ModMeta>(metaFile);
        
        if (string.IsNullOrEmpty(meta.Dependencies))
            return [];
        
        return meta.Dependencies.Split(';');
    }
    
    //Read cached data at startup if it exists
    public static void ReadCachedLocalLibraryData()
    {
        var path = GamePath.LocalLibrariesPath;
        
        if (!File.Exists(path))
            return;
        
        LibMeta.ReadLibMetaManifest(File.ReadAllText(path));
    }
    
    public static LibMeta GetLibMeta(string guid)
    {
        return LibraryMetaCache.FirstOrDefault(x => x.GUID.ToString() == guid);
    }
    
    public static bool DoesLibraryExist(string guid)
    {
        var libDir = GamePath.GetLibraryPath(guid);
        
        return Directory.Exists(libDir);
    }
    
    public static bool IsLibraryEnabled(string guid)
    {
        if (!DoesLibraryExist(guid))
            return false;
        
        return !File.Exists(Path.Combine(GamePath.GetLibraryPath(guid), ".disabled"));
    }
    
    public static string GetLibraryName(string guid)
    {
        var metaData = GetLibMeta(guid);
        
        if (metaData != null)
            return metaData.Name;
        
        var path = Path.Combine(GamePath.GetLibraryPath(guid), Constants.LIBRARIES_LIBMETA_FILE_NAME);
        
        if (!File.Exists(path))
            return guid;
        
        var yamlString = File.ReadAllText(path);
        var meta = LibMeta.ReadLibMeta(yamlString);
        
        return meta.Name;
    }
    
    public static async Task InstallLibraryAsync(string guid)
    {
        var metaFile = GetLibMeta(guid);
        
        if (metaFile == null || string.IsNullOrEmpty(metaFile.Download))
            return;
        
        var packagePath = await DownloadLibraryPackageAsync($"{guid}.zip", metaFile);
        
        var destDir = Path.Combine(GamePath.LibrariesPath, metaFile.GUID.ToString());
        
        if (Directory.Exists(destDir))
            Directory.Delete(destDir, true);
        
        Directory.CreateDirectory(destDir);
        
        await ZipFile.ExtractToDirectoryAsync(packagePath, destDir, true);
    }
    
    private static async Task<string> DownloadLibraryPackageAsync(string fileName, LibMeta meta)
    {
        Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Constants.TEMP_DIRECTORY_NAME));
        
        try
        {
            var path = Path.Combine(Path.GetTempPath(), Constants.TEMP_DIRECTORY_NAME, fileName);
            
            await using var stream = await Utils.Client.GetStreamAsync(meta.Download);
            await using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            
            await stream.CopyToAsync(fs);
            await fs.FlushAsync();
            
            return path;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to download library!");
            
            return string.Empty;
        }
    }
    
    public static async Task InstallAllModDependenciesAsync(List<ModInfo> mods)
    {
        try
        {
            await LibMeta.FetchAsync();
        }
        catch
        {
            // ignored
        }
        
        foreach (var mod in mods.Where(x => x.Enabled))
        {
            await InstallModDependenciesAsync(mod.Name);
        }
    }
    
    public static async Task InstallModDependenciesAsync(string mod)
    {
        var modDir = GamePath.GetModDirectory(mod);
        var metaFile = Path.Combine(modDir, "mod-meta.yaml");
        
        if (!Directory.Exists(modDir) || !File.Exists(metaFile))
            return;
        
        var meta = await YamlHelpers.DeserializeYamlFromPathAsync<ModMeta>(metaFile);
        
        if (string.IsNullOrEmpty(meta.Dependencies))
            return;
        
        foreach (var dep in meta.Dependencies.Split(';').Where(x => !DoesLibraryExist(x)))
        {
            await InstallLibraryAsync(dep);
        }
    }
}
