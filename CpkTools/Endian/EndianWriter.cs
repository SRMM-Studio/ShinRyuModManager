using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance;
using CpkTools.Model;

namespace CpkTools.Endian;

public sealed class EndianWriter : IDisposable
{
    public bool IsLittleEndian { get; set; }
    
    public long Position => BaseStream.Position;
    
    public Stream BaseStream { get; }
    
    private delegate void SpanWriter<in T>(Span<byte> span, T value);
    
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
        Write(BinaryPrimitives.WriteHalfLittleEndian, BinaryPrimitives.WriteHalfBigEndian, value);
    }
    
    public void Write(float value)
    {
        Write(BinaryPrimitives.WriteSingleLittleEndian, BinaryPrimitives.WriteSingleBigEndian, value);
    }
    
    public void Write(double value)
    {
        Write(BinaryPrimitives.WriteDoubleLittleEndian, BinaryPrimitives.WriteDoubleBigEndian, value);
    }
    
    public void Write(short value)
    {
        Write(BinaryPrimitives.WriteInt16LittleEndian, BinaryPrimitives.WriteInt16BigEndian, value);
    }
    
    public void Write(int value)
    {
        Write(BinaryPrimitives.WriteInt32LittleEndian, BinaryPrimitives.WriteInt32BigEndian, value);
    }
    
    public void Write(long value)
    {
        Write(BinaryPrimitives.WriteInt64LittleEndian, BinaryPrimitives.WriteInt64BigEndian, value);
    }
    
    public void Write(ushort value)
    {
        Write(BinaryPrimitives.WriteUInt16LittleEndian, BinaryPrimitives.WriteUInt16BigEndian, value);
    }
    
    public void Write(uint value)
    {
        Write(BinaryPrimitives.WriteUInt32LittleEndian, BinaryPrimitives.WriteUInt32BigEndian, value);
    }
    
    public void Write(ulong value)
    {
        Write(BinaryPrimitives.WriteUInt64LittleEndian, BinaryPrimitives.WriteUInt64BigEndian, value);
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
            throw new NotSupportedException("Not supported type!");
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
    
    // Helper method, since all Write methods are the same shape
    private void Write<T>(SpanWriter<T> le, SpanWriter<T> be, T value) where T : struct
    {
        Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<T>()];
        
        (IsLittleEndian ? le : be)(buffer, value);
        
        BaseStream.Write(buffer);
    }
}
