namespace ShinRyuModManager;

public readonly record struct ParlessFile(string Name, int Index);
public readonly record struct ParlessFolder(string Name, int Index);
public record CpkFolder(string Name, List<ushort> Indices);
