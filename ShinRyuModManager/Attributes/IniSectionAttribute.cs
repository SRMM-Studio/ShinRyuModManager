using JetBrains.Annotations;

namespace ShinRyuModManager.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class IniSectionAttribute(string name) : Attribute
{
    public string Name { get; } = name;
    public string Comment { get; init; }
}
