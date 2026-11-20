using ShinRyuModManager.Attributes;

namespace ShinRyuModManager.Config.Models.Sections;

public sealed class ParlessSection
{
    [IniKey("IniVersion", Hidden = true, Comment = "Ini version. Should not be changed manually.")]
    public int IniVersion { get; set; } = ParlessConfig.CURRENT_VERSION;
    
    [IniKey("ParlessEnabled", Comment = "Global switch for Parless. Set to 0 to disable everything.")]
    public bool ParlessEnabled { get; set; } = true;
    
    [IniKey("TempDisabled", Comment = "Temporarily disables Parless for one run only. Overrides ParlessEnabled.\n" +
        "This will be set back to 0 whenever the game is launched with it set to 1.")]
    public bool TempDisabled { get; set; } = false;
}
