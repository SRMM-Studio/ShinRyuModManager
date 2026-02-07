namespace CpkTools.Model;

public sealed record FileEntry
{
    public string DirName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    
    public ulong FileSize { get; set; }
    public long FileSizePos { get; set; }
    public Type? FileSizeType { get; set; }
    
    public ulong ExtractSize { get; set; } // int
    public long ExtractSizePos { get; set; }
    public Type? ExtractSizeType { get; set; }
    
    public ulong FileOffset { get; set; }
    public long FileOffsetPos { get; set; }
    public Type? FileOffsetType { get; set; }
    
    public ulong Offset { get; set; }
    public int Id { get; set; }
    public string UserString { get; set; } = string.Empty;
    public ulong UpdateDateTime { get; set; } = 0;
    public string LocalDir { get; set; } = string.Empty;
    public string TocName { get; set; } = string.Empty;
    
    public bool Encrypted { get; set; }
    
    public string FileType { get; set; } = string.Empty;
}
