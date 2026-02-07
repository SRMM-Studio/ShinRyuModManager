namespace ShinRyuModManager.Templates;

public readonly struct IniKey
{
    public string Name { get; init; }
    
    public List<string> Comments { get; init; }
    
    public int DefaultValue { get; init; }
}
