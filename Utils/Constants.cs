namespace Utils;

public static class Constants
{
    // File Names
    public const string INI = "YakuzaParless.ini";
    public const string TXT = "ModList.txt";
    public const string TXT_OLD = "ModLoadOrder.txt";
    public const string MOD_LIST = "ModList.ml";
    public const string MLO = "YakuzaParless.mlo";
    public const string ASI = "YakuzaParless.asi";
    public const string DINPUT8DLL = "dinput8.dll";
    public const string VERSIONDLL = "version.dll";
    public const string WINMMDLL = "winmm.dll";
    public const string WINMMLJ = "winmm.lj";
    public const string PARLESS_NAME = ".parless paths";
    public const string EXTERNAL_MODS = "_externalMods";
    public const string VORTEX_MANAGED_FILE = "__folder_managed_by_vortex";
    
    public static readonly string PARLESS_MODS_PATH = Path.Combine("mods", "Parless");
    
    public static readonly List<string> IncompatiblePars =
    [
        "chara",
        "map_",
        "effect",
        "pausepar",
        "2d",
        "cse",
        "prep",
        "light_anim",
        "particle",
        "stage"
    ];
    
    // Updates
    public const string UPDATER_EXECUTABLE_NAME = "RyuUpdater.exe";
    public const string UPDATE_FLAG_FILE_NAME = "update.txt";
    public const string UPDATE_RECENT_FLAG_FILE_NAME = ".SRMM_RECENT_UPDATE_FLAG";
    public const string UPDATE_INFO_REPO_OWNER = "SRMM-Studio";
    public const string UPDATE_INFO_REPO = "srmm-version-info";
    public const string UPDATE_INFO_FILE_PATH = "RyuUpdater/config.yaml";
    
    //TEMP
    public const string TEMP_DIRECTORY_NAME = "ShinRyuModManager";
    
    //EVENTS
    public const string EVENT_FOOLS24_FLAG_FILE_NAME = ".SRMM_FOOLS24_FLAG";
    
    //LIBRARIES
    public const string LIBRARIES_INFO_REPO_OWNER = "SRMM-Studio";
    public const string LIBRARIES_INFO_REPO = "srmm-lib-info";
    public const string LIBRARIES_INFO_REPO_FILE_PATH = "libraries.yaml";
    public const string LIBRARIES_LIBMETA_FILE_NAME = "lib-meta.yaml";
    
    //LOGGING
    public const string LOGS_BASE_PATH = "srmm-logs";
}
