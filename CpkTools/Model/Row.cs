namespace CpkTools.Model;

public record struct Row()
{
    public int Type { get; set; } = -1;
    
    //column based datatypes
    public byte UInt8 { get; set; }
    public ushort UInt16 { get; set; }
    public uint UInt32 { get; set; }
    public ulong UInt64 { get; set; }
    public float UFloat { get; set; }
    public string Str { get; set; } = string.Empty;
    public byte[] Data { get; set; } = [];
    public long Position { get; set; }
    
    public object? GetValue()
    {
        return Type switch
        {
            0 or 1 => UInt8,
            2 or 3 => UInt16,
            4 or 5 => UInt32,
            6 or 7 => UInt64,
            8 => UFloat,
            0xA => Str,
            0xB => Data,
            _ => null
        };
    }
    
    public new Type? GetType()
    {
        return Type switch
        {
            0 or 1 => UInt8.GetType(),
            2 or 3 => UInt16.GetType(),
            4 or 5 => UInt32.GetType(),
            6 or 7 => UInt64.GetType(),
            8 => UFloat.GetType(),
            0xA => Str.GetType(),
            0xB => Data.GetType(),
            _ => null
        };
    }
}
