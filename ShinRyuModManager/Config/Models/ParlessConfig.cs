using ShinRyuModManager.Attributes;
using ShinRyuModManager.Config.Models.Sections;

namespace ShinRyuModManager.Config.Models;

public sealed class ParlessConfig
{
    public const int CURRENT_VERSION = 7;
    
    [IniSection("Parless", Comment = "All values are 0 for false, 1 for true.")]
    public ParlessSection Parless { get; set; } = new();
    
    [IniSection("Overrides", Comment = "General override order:\n" +
        "if LooseFilesEnabled is set to 1, files inside \".parless\" paths will override everything.\n" +
        "if ModsEnabled is set to 1, mod files will override files inside pars.")]
    public OverridesSection Overrides { get; set; } = new();
    
    [IniSection("RyuModManager")]
    public ModManagerSection ModManager { get; set; } = new();
    
    [IniSection("Logs")]
    public LogsSection Logs { get; set; } = new();
    
    [IniSection("Debug")]
    public DebugSection Debug { get; set; } = new();
}
