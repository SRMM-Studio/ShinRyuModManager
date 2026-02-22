using CpkTools.Endian;

namespace CpkTools.Model;

public class Utf
{
    public List<Column>? Columns { get; private set; }
    public Row[][]? Rows { get; private set; }
    
    private int _tableSize;
    private long _rowsOffset;
    private long _stringsOffset;
    private long _dataOffset;
    private int _tableName;
    private short _numColumns;
    private short _rowLength;
    private int _numRows;
    
    public bool ReadUtf(Memory<byte> data)
    {
        var reader = new EndianReader(data);
        
        return ReadUtf(reader);
    }
    
    public bool ReadUtf(byte[] data)
    {
        var reader = new EndianReader(data);
        
        return ReadUtf(reader);
    }
    
    public bool ReadUtf(Stream stream)
    {
        var reader = new EndianReader(stream);
        
        return ReadUtf(reader);
    }
    
    private bool ReadUtf(EndianReader reader, bool leaveOpen = false)
    {
        var offset = reader.Position;
        
        if (Tools.ReadCString(reader, 4) != "@UTF")
            return false;
        
        _tableSize = reader.ReadInt32();
        _rowsOffset = reader.ReadInt32();
        _stringsOffset = reader.ReadInt32();
        _dataOffset = reader.ReadInt32();
        
        // CPK Header & UTF Header are ignored, so add 8 to each offset
        _rowsOffset += offset + 8;
        _stringsOffset += offset + 8;
        _dataOffset += offset + 8;
        
        _tableName = reader.ReadInt32();
        _numColumns = reader.ReadInt16();
        _rowLength = reader.ReadInt16();
        _numRows = reader.ReadInt32();
        
        Columns = new List<Column>(_numColumns);
        Rows = new Row[_numRows][];
        
        // Read Columns
        for (var i = 0; i < _numColumns; i++)
        {
            var column = new Column
            {
                Flags = reader.ReadByte()
            };
            
            if (column.Flags == 0)
            {
                reader.Seek(3, SeekOrigin.Current);
                column.Flags = reader.ReadByte();
            }
            
            column.Name = Tools.ReadCString(reader, -1, reader.ReadInt32() + _stringsOffset);
            Columns.Add(column);
        }
        
        const int storageMask = (int)StorageFlags.StorageMask;
        const int typeMask = (int)TypeFlags.TypeMask;
        
        // Read Rows
        for (var y = 0; y < _numRows; y++)
        {
            reader.Seek(_rowsOffset + (y * _rowLength), SeekOrigin.Begin);
            
            var currentEntry = new Row[_numColumns];
            
            for (var x = 0; x < _numColumns; x++)
            {
                var currentRow = new Row();
                var storageFlag = Columns[x].Flags & storageMask;
                
                switch (storageFlag)
                {
                    case (int)StorageFlags.StorageNone:
                    case (int)StorageFlags.StorageZero:
                    case (int)StorageFlags.StorageConstant:
                        currentEntry[x] = currentRow;
                        
                        continue;
                }
                
                // 0x50
                currentRow.Type = Columns[x].Flags & typeMask;
                currentRow.Position = reader.Position;
                
                switch (currentRow.Type)
                {
                    case 0 or 1:
                        currentRow.UInt8 = reader.ReadByte();
                        
                        break;
                    case 2 or 3:
                        currentRow.UInt16 = reader.ReadUInt16();
                        
                        break;
                    case 4 or 5:
                        currentRow.UInt32 = reader.ReadUInt32();
                        
                        break;
                    case 6 or 7:
                        currentRow.UInt64 = reader.ReadUInt64();
                        
                        break;
                    case 8:
                        currentRow.UFloat = reader.ReadSingle();
                        
                        break;
                    case 0xA:
                        currentRow.Str = Tools.ReadCString(reader, -1, reader.ReadInt32() + _stringsOffset);
                        
                        break;
                    case 0xB:
                        var position = reader.ReadInt32() + _dataOffset;
                        
                        currentRow.Position = position;
                        currentRow.Data = Tools.GetData(reader, position, reader.ReadInt32());
                        
                        break;
                    default:
                        throw new NotImplementedException();
                }
                
                currentEntry[x] = currentRow;
            }
            
            Rows[y] = currentEntry;
        }
        
        if (!leaveOpen)
        {
            reader.Dispose();
        }
        
        return true;
    }
}
