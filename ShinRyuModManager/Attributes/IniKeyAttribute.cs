namespace ShinRyuModManager.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class IniKeyAttribute(string name) : Attribute
{
    public string Name { get; } = name;
    // UI Label
    public string Display { get; init; }
    // File comment
    public string Comment { get; init; }
    // Skipped in UI (IniVersion)
    public bool Hidden { get; init; }
}
