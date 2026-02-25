namespace ShinRyuModManager.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class DescriptionAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
