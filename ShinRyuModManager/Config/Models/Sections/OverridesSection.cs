using ShinRyuModManager.Attributes;

namespace ShinRyuModManager.Config.Models.Sections;

public sealed class OverridesSection
{
    [IniKey("LooseFilesEnabled", Comment = "Allows loading files from \".parless\" paths.\n" +
        "Files in these paths will override the mod files installed in /mods/\n" +
        "Example: files in /data/chara.parless/ will override the\n" +
        "files in /data/chara.par AND files in /chara/ in all mods.")]
    public bool LooseFilesEnabled { get; set; } = false;
    
    [IniKey("ModsEnabled", Comment = "Allows loading files from the /mods/ directory.\n" +
        "Each mod has to be extracted in its own folder, where its contents\n" +
        "will mirror the game's /data/ directory. Pars should be extracted into folders.\n" +
        "Example: /mods/ExampleMod/chara/auth/c_am_kiryu/c_am_kiryu.gmd\n" +
        "will replace the /auth/c_am_kiryu/c_am_kiryu.gmd file inside /data/chara.par")]
    public bool ModsEnabled { get; set; } = true;
    
    [IniKey("RebuildMLO", Comment = "Removes the need to run ShinRyuModManager before launching your game,\n" +
        "should have little to no effect on the time it takes to launch,\n" +
        "and should help users avoid mistakenly not rebuilding.\n" +
        "Optional QOL feature to help you avoid having to re-run the mod manager every time.")]
    public bool RebuildMlo { get; set; } = true;
    
    [IniKey("Locale", Comment = "Changes the filepaths of localized pars to match the current locale.\n" +
        "Only needed if you're running a non-English version of the game.\n" +
        "English=0, Japanese=1, Chinese=2, Korean=3")]
    public Locale Locale { get; set; } = Locale.English;
}
