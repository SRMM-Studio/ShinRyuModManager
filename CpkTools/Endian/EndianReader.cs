using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance;

namespace CpkTools.Endian;

public sealed class EndianReader : IDisposable
{
    public bool IsLittleEndian { get; set; }
    
    public long Position => BaseStream.Position;
    
    public Stream BaseStream { get; }
    
    private delegate TRet SpanReader<TIn, out TRet>(ReadOnlySpan<TIn> span);
    
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
        return Read(BinaryPrimitives.ReadHalfLittleEndian, BinaryPrimitives.ReadHalfBigEndian);
    }
    
    public float ReadSingle()
    {
        return Read(BinaryPrimitives.ReadSingleLittleEndian, BinaryPrimitives.ReadSingleBigEndian);
    }
    
    public double ReadDouble()
    {
        return Read(BinaryPrimitives.ReadDoubleLittleEndian, BinaryPrimitives.ReadDoubleBigEndian);
    }
    
    public short ReadInt16()
    {
        return Read(BinaryPrimitives.ReadInt16LittleEndian, BinaryPrimitives.ReadInt16BigEndian);
    }
    
    public int ReadInt32()
    {
        return Read(BinaryPrimitives.ReadInt32LittleEndian, BinaryPrimitives.ReadInt32BigEndian);
    }
    
    public long ReadInt64()
    {
        return Read(BinaryPrimitives.ReadInt64LittleEndian, BinaryPrimitives.ReadInt64BigEndian);
    }
    
    public ushort ReadUInt16()
    {
        return Read(BinaryPrimitives.ReadUInt16LittleEndian, BinaryPrimitives.ReadUInt16BigEndian);
    }
    
    public uint ReadUInt32()
    {
        return Read(BinaryPrimitives.ReadUInt32LittleEndian, BinaryPrimitives.ReadUInt32BigEndian);
    }
    
    public ulong ReadUInt64()
    {
        return Read(BinaryPrimitives.ReadUInt64LittleEndian, BinaryPrimitives.ReadUInt64BigEndian);
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
        
        var buffer = ArrayPool<byte>.Shared.Rent(80 * 1024);
        var remaining = length;
        var totalRead = 0;
        
        try
        {
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
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
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
    
    // Helper method, since all ReadX methods are the same shape
    private T Read<T>(SpanReader<byte, T> le, SpanReader<byte, T> be) where T : struct
    {
        Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<T>()];
        
        BaseStream.ReadExactly(buffer);
        
        return IsLittleEndian ? le(buffer) : be(buffer);
    }
}
