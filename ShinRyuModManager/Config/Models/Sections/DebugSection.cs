using ShinRyuModManager.Attributes;

namespace ShinRyuModManager.Config.Models.Sections;

public sealed class DebugSection
{
    [IniKey("ConsoleEnabled", Comment = "Enable the debugging console")]
    public bool ConsoleEnabled { get; set; } = false;
    
    [IniKey("ReloadingEnabled", Comment = "Enable reloading mod files by pressing CTRL+ALT+R")]
    public bool ReloadingEnabled { get; set; } = false;
    
    [IniKey("V5FSArcadeSupport", Comment = "Enable modding Virtua Fighter 5 Final Showdown on Yakuza games who have it on their arcade.")]
    public bool V5FSArcadeSupport { get; set; } = false;
}
