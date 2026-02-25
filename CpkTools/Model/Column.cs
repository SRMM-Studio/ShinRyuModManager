namespace CpkTools.Model;

public record struct Column
{
    public string Name { get; set; }
    public byte Flags { get; set; }
}
