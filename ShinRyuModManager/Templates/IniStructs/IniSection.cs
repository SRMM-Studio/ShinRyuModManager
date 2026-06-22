namespace ShinRyuModManager.Templates;

public readonly struct IniSection
{
    public string Name { get; init; }
    
    public List<string> Comments { get; init; }
    
    public List<IniKey> Keys { get; init; }
}
