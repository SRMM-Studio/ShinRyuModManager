using ShinRyuModManager.Attributes;

namespace ShinRyuModManager.Config.Models.Sections;

public sealed class LogsSection
{
    [IniKey("LogMods", Comment = "Write filepaths for mods that get loaded into modOverrides.txt")]
    public bool LogMods { get; set; } = false;
    
    [IniKey("LogParless", Comment = "Write filepaths for .parless paths that get loaded into parlessOverrides.txt")]
    public bool LogParless { get; set; } = false;
    
    [IniKey("LogAll", Comment = "Write filepaths for every file that gets loaded into allFilepaths.txt")]
    public bool LogAll { get; set; } = false;
    
    [IniKey("IgnoreNonPaths", Comment = "Do not log any strings that are not proper filepaths")]
    public bool IgnoreNonPaths { get; set; } = true;
}
