using System.Buffers.Binary;
using CommunityToolkit.HighPerformance;

namespace CpkTools.Endian;

public sealed class EndianReader : IDisposable
{
    public bool IsLittleEndian { get; set; }
    
    public long Position
    {
        get => BaseStream.Position;
    }
    
    public Stream BaseStream { get; }
    
    public EndianReader(Memory<byte> data, bool isLittleEndian = false)
    {
        BaseStream = data.AsStream();
        IsLittleEndian = isLittleEndian;
    }
    
    public EndianReader(byte[] data, bool isLittleEndian = false)
    {
        BaseStream = new MemoryStream(data);
        IsLittleEndian = isLittleEndian;
    }
    
    public EndianReader(Stream stream, bool isLittleEndian = false)
    {
        BaseStream = stream;
        IsLittleEndian = isLittleEndian;
    }
    
    public Half ReadHalf()
    {
        Span<byte> buffer = stackalloc byte[2];
        
        BaseStream.ReadExactly(buffer);
        
        return IsLittleEndian
            ? BinaryPrimitives.ReadHalfLittleEndian(buffer)
            : BinaryPrimitives.ReadHalfBigEndian(buffer);
    }
    
    public float ReadSingle()
    {
        Span<byte> buffer = stackalloc byte[4];
        
        BaseStream.ReadExactly(buffer);
        
        return IsLittleEndian
            ? BinaryPrimitives.ReadSingleLittleEndian(buffer)
            : BinaryPrimitives.ReadSingleBigEndian(buffer);
    }
    
    public double ReadDouble()
    {
        Span<byte> buffer = stackalloc byte[8];
        
        BaseStream.ReadExactly(buffer);
        
        return IsLittleEndian
            ? BinaryPrimitives.ReadDoubleLittleEndian(buffer)
            : BinaryPrimitives.ReadDoubleBigEndian(buffer);
    }
    
    public short ReadInt16()
    {
        Span<byte> buffer = stackalloc byte[2];
        
        BaseStream.ReadExactly(buffer);
        
        return IsLittleEndian
            ? BinaryPrimitives.ReadInt16LittleEndian(buffer)
            : BinaryPrimitives.ReadInt16BigEndian(buffer);
    }
    
    public int ReadInt32()
    {
        Span<byte> buffer = stackalloc byte[4];
        
        BaseStream.ReadExactly(buffer);
        
        return IsLittleEndian
            ? BinaryPrimitives.ReadInt32LittleEndian(buffer)
            : BinaryPrimitives.ReadInt32BigEndian(buffer);
    }
    
    public long ReadInt64()
    {
        Span<byte> buffer = stackalloc byte[8];
        
        BaseStream.ReadExactly(buffer);
        
        return IsLittleEndian
            ? BinaryPrimitives.ReadInt64LittleEndian(buffer)
            : BinaryPrimitives.ReadInt64BigEndian(buffer);
    }
    
    public ushort ReadUInt16()
    {
        Span<byte> buffer = stackalloc byte[2];
        
        BaseStream.ReadExactly(buffer);
        
        return IsLittleEndian
            ? BinaryPrimitives.ReadUInt16LittleEndian(buffer)
            : BinaryPrimitives.ReadUInt16BigEndian(buffer);
    }
    
    public uint ReadUInt32()
    {
        Span<byte> buffer = stackalloc byte[4];
        
        BaseStream.ReadExactly(buffer);
        
        return IsLittleEndian
            ? BinaryPrimitives.ReadUInt32LittleEndian(buffer)
            : BinaryPrimitives.ReadUInt32BigEndian(buffer);
    }
    
    public ulong ReadUInt64()
    {
        Span<byte> buffer = stackalloc byte[8];
        
        BaseStream.ReadExactly(buffer);
        
        return IsLittleEndian
            ? BinaryPrimitives.ReadUInt64LittleEndian(buffer)
            : BinaryPrimitives.ReadUInt64BigEndian(buffer);
    }
    
    public byte ReadByte()
    {
        var value = BaseStream.ReadByte();
        
        if (value == -1)
            throw new EndOfStreamException();
        
        return (byte)value;
    }
    
    public byte[] ReadBytes(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        
        if (count == 0)
        {
            return [];
        }
        
        var result = new byte[count];
        var numRead = BaseStream.ReadAtLeast(result, result.Length, throwOnEndOfStream: false);
        
        if (numRead != result.Length)
        {
            // Trim array. This should happen on EOF & possibly net streams.
            result = result[..numRead];
        }
        
        return result;
    }
    
    public int ReadStreamInto(Stream dest, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        
        if (length == 0)
        {
            return 0;
        }
        
        var buffer = new byte[80 * 1024];
        var remaining = length;
        var totalRead = 0;
        
        while (remaining > 0)
        {
            var toRead = Math.Min(buffer.Length, remaining);
            var read = BaseStream.Read(buffer, 0, toRead);
            
            if (read == 0) // EOF
                break;
            
            dest.Write(buffer, 0, read);
            totalRead += read;
            remaining -= read;
        }
        
        return totalRead;
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
