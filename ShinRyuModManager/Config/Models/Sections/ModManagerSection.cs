using ShinRyuModManager.Attributes;

namespace ShinRyuModManager.Config.Models.Sections;

public sealed class ModManagerSection
{
    [IniKey("Verbose", Comment = "Print additional info, including all file paths that get added to the MLO")]
    public bool Verbose { get; set; } = false;
    
    [IniKey("ShowWarnings", Comment = "Show warnings whenever a mod was possibly not extracted correctly")]
    public bool ShowWarnings { get; set; } = true;
    
    [IniKey("LoadExternalModsOnly", Comment = "Only load mods from the /mods/_externalMods/ directory, and ignore the load order file.\n" +
        "That directory will be created automatically by external mod managers.\n" +
        "If this option is disabled, external mods will NOT be loaded, unless \"_externalMods\" is added manually to the load order file.\n" +
        "If the directory does not exist, then the load order will be used as normal.")]
    public bool LoadExternalModsOnly { get; set; } = true;
}
