using System.Buffers.Binary;
using CommunityToolkit.HighPerformance;
using CpkTools.Model;

namespace CpkTools.Endian;

public sealed class EndianWriter : IDisposable
{
    public bool IsLittleEndian { get; set; }
    
    public long Position
    {
        get => BaseStream.Position;
    }
    
    public Stream BaseStream { get; }
    
    public EndianWriter(Memory<byte> data, bool isLittleEndian = false)
    {
        BaseStream = data.AsStream();
        IsLittleEndian = isLittleEndian;
    }
    
    public EndianWriter(Stream stream, bool isLittleEndian = false)
    {
        BaseStream = stream;
        IsLittleEndian = isLittleEndian;
    }
    
    public void Write(bool value)
    {
        BaseStream.WriteByte(Convert.ToByte(value));
    }
    
    public void Write(char value)
    {
        Write((ushort)value);
    }
    
    public void Write(Half value)
    {
        Span<byte> buffer = stackalloc byte[2];
        
        if (IsLittleEndian)
        {
            BinaryPrimitives.WriteHalfLittleEndian(buffer, value);
        }
        else
        {
            BinaryPrimitives.WriteHalfBigEndian(buffer, value);
        }
        
        BaseStream.Write(buffer);
    }
    
    public void Write(float value)
    {
        Span<byte> buffer = stackalloc byte[4];
        
        if (IsLittleEndian)
        {
            BinaryPrimitives.WriteSingleLittleEndian(buffer, value);
        }
        else
        {
            BinaryPrimitives.WriteSingleBigEndian(buffer, value);
        }
        
        BaseStream.Write(buffer);
    }
    
    public void Write(double value)
    {
        Span<byte> buffer = stackalloc byte[8];
        
        if (IsLittleEndian)
        {
            BinaryPrimitives.WriteDoubleLittleEndian(buffer, value);
        }
        else
        {
            BinaryPrimitives.WriteDoubleBigEndian(buffer, value);
        }
        
        BaseStream.Write(buffer);
    }
    
    public void Write(short value)
    {
        Span<byte> buffer = stackalloc byte[2];
        
        if (IsLittleEndian)
        {
            BinaryPrimitives.WriteInt16LittleEndian(buffer, value);
        }
        else
        {
            BinaryPrimitives.WriteInt16BigEndian(buffer, value);
        }
        
        BaseStream.Write(buffer);
    }
    
    public void Write(int value)
    {
        Span<byte> buffer = stackalloc byte[4];
        
        if (IsLittleEndian)
        {
            BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
        }
        else
        {
            BinaryPrimitives.WriteInt32BigEndian(buffer, value);
        }
        
        BaseStream.Write(buffer);
    }
    
    public void Write(long value)
    {
        Span<byte> buffer = stackalloc byte[8];
        
        if (IsLittleEndian)
        {
            BinaryPrimitives.WriteInt64LittleEndian(buffer, value);
        }
        else
        {
            BinaryPrimitives.WriteInt64BigEndian(buffer, value);
        }
        
        BaseStream.Write(buffer);
    }
    
    public void Write(ushort value)
    {
        Span<byte> buffer = stackalloc byte[2];
        
        if (IsLittleEndian)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(buffer, value);
        }
        else
        {
            BinaryPrimitives.WriteUInt16BigEndian(buffer, value);
        }
        
        BaseStream.Write(buffer);
    }
    
    public void Write(uint value)
    {
        Span<byte> buffer = stackalloc byte[4];
        
        if (IsLittleEndian)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(buffer, value);
        }
        else
        {
            BinaryPrimitives.WriteUInt32BigEndian(buffer, value);
        }
        
        BaseStream.Write(buffer);
    }
    
    public void Write(ulong value)
    {
        Span<byte> buffer = stackalloc byte[8];
        
        if (IsLittleEndian)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(buffer, value);
        }
        else
        {
            BinaryPrimitives.WriteUInt64BigEndian(buffer, value);
        }
        
        BaseStream.Write(buffer);
    }
    
    public void Write(byte value)
    {
        BaseStream.WriteByte(value);
    }
    
    public void Write(sbyte value)
    {
        BaseStream.WriteByte((byte)value);
    }
    
    public void Write(byte[] value)
    {
        BaseStream.Write(value);
    }
    
    public void Write(Span<byte> value)
    {
        BaseStream.Write(value);
    }
    
    public void Write(ReadOnlySpan<byte> value)
    {
        BaseStream.Write(value);
    }
    
    public void Write(Stream stream)
    {
        stream.CopyTo(BaseStream);
    }
    
    public void Write(FileEntry entry)
    {
        if (entry.ExtractSizeType == typeof(byte))
        {
            Write((byte)entry.ExtractSize);
        }
        else if (entry.ExtractSizeType == typeof(ushort))
        {
            Write((ushort)entry.ExtractSize);
        }
        else if (entry.ExtractSizeType == typeof(uint))
        {
            Write((uint)entry.ExtractSize);
        }
        else if (entry.ExtractSizeType == typeof(ulong))
        {
            Write(entry.ExtractSize);
        }
        else if (entry.ExtractSizeType == typeof(float))
        {
            Write((float)entry.ExtractSize);
        }
        else
        {
            throw new Exception("Not supported type!");
        }
    }
    
    public void Seek(long offset, SeekOrigin origin)
    {
        BaseStream.Seek(offset, origin);
    }
    
    public void Dispose()
    {
        BaseStream.Dispose();
    }
}
