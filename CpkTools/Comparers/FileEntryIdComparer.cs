using CpkTools.Model;

namespace CpkTools.Comparers;

public sealed class FileEntryIdComparer : IComparer<FileEntry>
{
    public static readonly FileEntryIdComparer Instance = new();
    
    public int Compare(FileEntry? x, FileEntry? y)
    {
        if (ReferenceEquals(x, y))
            return 0;
        
        if (x is null)
            return -1;
        
        if (y is null)
            return 1;
        
        return x.Id.CompareTo(y.Id);
    }
}
