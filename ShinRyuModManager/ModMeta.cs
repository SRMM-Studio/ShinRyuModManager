namespace ShinRyuModManager;

public class ModMeta
{
    public string Name { get; internal set; }
    public string Author { get; internal set; }
    public string Version { get; internal set; }
    public string Description { get; internal set; }
    public string Dependencies { get; internal set; }
    
    public static ModMeta GetPlaceholder()
    {
        return new ModMeta
        {
            Name = "Mod Name",
            Author = "Author",
            Version = "1.0.0",
            Description = "Mod description",
            Dependencies = ""
        };
    }
    
    public static ModMeta GetPlaceholder(string modName)
    {
        var meta = GetPlaceholder();
        
        meta.Name = modName;
        
        return meta;
    }
}
