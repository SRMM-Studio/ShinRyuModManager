using System.Buffers.Binary;
using System.Numerics;
using System.Text;
using CpkTools.Comparers;
using CpkTools.Endian;

namespace CpkTools.Model;

public sealed class Cpk
{
    public List<FileEntry> FileTable { get; } = [];
    public ulong ContentOffset { get; private set; }
    
    private bool _isUtfEncrypted;
    
    //private int _unk1;
    private long _utfSize;
    private Memory<byte> _utfPacket;
    private Memory<byte> _cpkPacket;
    private Memory<byte> _tocPacket;
    private Memory<byte> _itocPacket;
    private Memory<byte> _etocPacket;
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    private Memory<byte> _gtocPacket;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
    private ulong _tocOffset = ulong.MaxValue;
    private ulong _etocOffset = ulong.MaxValue;
    private ulong _itocOffset = ulong.MaxValue;
    private ulong _gtocOffset = ulong.MaxValue;
    private Utf? _utf;
    private Utf? _files;
    
    private static ReadOnlySpan<byte> UtfHeader
    {
        get => "@UTF"u8;
    }
    
    public bool ReadCpk(string path)
    {
        if (!File.Exists(path))
            return false;
        
        using var reader = new EndianReader(File.OpenRead(path), true);
        
        if (Tools.ReadCString(reader, 4) != "CPK ")
        {
            return false;
        }
        
        ReadUtfData(reader);
        
        _cpkPacket = _utfPacket;
        
        // Dump CPK
        var cpkEntry = new FileEntry
        {
            FileName = "CPK_HDR",
            FileOffsetPos = reader.Position + 0x10,
            FileSize = (ulong)_cpkPacket.Length,
            Encrypted = _isUtfEncrypted,
            FileType = "CPK"
        };
        
        FileTable.Add(cpkEntry);
        
        _utf = new Utf();
        
        if (!_utf.ReadUtf(_utfPacket))
        {
            return false;
        }
        
        /*try {
            for (var i = 0; i < _utf.Columns.Count; i++) {
                _cpkData.Add(_utf.Columns[i].Name, _utf.Rows[0][i].GetValue()!);
            }
        } catch (Exception ex) {
            Console.WriteLine(ex);
        }*/
        
        ContentOffset = TryGetColumnData<ulong>(_utf, 0, "ContentOffset", out var content) ? content.Value : ulong.MaxValue;
        
        var newEntry = CreateFileEntry("CONTENT_OFFSET", ContentOffset, content.Position, "CPK", "CONTENT", false);
        
        FileTable.Add(newEntry);
        
        var align = TryGetColumnData<ushort>(_utf, 0, "Align", out var alignData) ? alignData.Value : ushort.MaxValue;
        
        if (TryGetColumnData<ulong>(_utf, 0, "TocOffset", out var toc))
        {
            var entry = CreateFileEntry("TOC_HDR", toc.Value, toc.Position, "CPK", "HDR", false);
            
            FileTable.Add(entry);
            
            _tocOffset = toc.Value;
            
            if (!ReadToc(reader, toc.Value, ContentOffset))
                return false;
        }
        
        if (TryGetColumnData<ulong>(_utf, 0, "EtocOffset", out var etoc))
        {
            var entry = CreateFileEntry("ETOC_HDR", etoc.Value, etoc.Position, "CPK", "HDR", false);
            
            FileTable.Add(entry);
            
            _etocOffset = etoc.Value;
            
            if (!ReadEtoc(reader, etoc.Value))
                return false;
        }
        
        if (TryGetColumnData<ulong>(_utf, 0, "ItocOffset", out var itoc))
        {
            var entry = CreateFileEntry("ITOC_HDR", itoc.Value, itoc.Position, "CPK", "HDR", false);
            
            FileTable.Add(entry);
            
            _itocOffset = itoc.Value;
            
            if (!ReadItoc(reader, itoc.Value, ContentOffset, align))
                return false;
        }
        
        if (TryGetColumnData<ulong>(_utf, 0, "GtocOffset", out var gtoc))
        {
            var entry = CreateFileEntry("GTOC_HDR", gtoc.Value, gtoc.Position, "CPK", "HDR", false);
            
            FileTable.Add(entry);
            
            _gtocOffset = gtoc.Value;
            
            if (!ReadGtoc(reader, gtoc.Value))
                return false;
        }
        
        _files = null;
        
        return true;
    }
    
    public static byte[] DecompressCrilayla(byte[] input, int size)
    {
        using var reader = new EndianReader(input);
        
        reader.Seek(8, SeekOrigin.Begin); // Skip CRILAYLA
        
        var uncompressedSize = reader.ReadInt32();
        var uncompressedHeaderOffset = reader.ReadInt32();
        var result = new byte[uncompressedSize + 0x100];
        
        // Copy uncompressed 0x100 header to start of file
        Array.Copy(input, uncompressedHeaderOffset + 0x10, result, 0, 0x100);
        
        var inputEnd = input.Length - 0x100 - 1;
        var inputOffset = inputEnd;
        var outputEnd = 0x100 + uncompressedSize - 1;
        byte bitPool = 0;
        var bitsLeft = 0;
        var bytesOutput = 0;
        int[] vleLens = [2, 3, 5, 8];
        
        while (bytesOutput < uncompressedSize)
        {
            if (GenNextBits(input, ref inputOffset, ref bitPool, ref bitsLeft, 1) > 0)
            {
                var backreferenceOffset = outputEnd - bytesOutput +
                    GenNextBits(input, ref inputOffset, ref bitPool, ref bitsLeft, 13) + 3;
                
                var backreferenceLength = 3;
                int vleLevel;
                
                for (vleLevel = 0; vleLevel < vleLens.Length; vleLevel++)
                {
                    int thisLevel = GenNextBits(input, ref inputOffset, ref bitPool, ref bitsLeft, vleLens[vleLevel]);
                    
                    backreferenceLength += thisLevel;
                    
                    if (thisLevel != ((1 << vleLens[vleLevel]) - 1))
                        break;
                }
                
                if (vleLevel == vleLens.Length)
                {
                    int thisLevel;
                    
                    do
                    {
                        thisLevel = GenNextBits(input, ref inputOffset, ref bitPool, ref bitsLeft, 8);
                        
                        backreferenceLength += thisLevel;
                    } while (thisLevel == 255);
                }
                
                for (var i = 0; i < backreferenceLength; i++)
                {
                    result[outputEnd - bytesOutput] = result[backreferenceOffset--];
                    bytesOutput++;
                }
            }
            else
            {
                // Verbatim byte
                result[outputEnd - bytesOutput] = (byte)GenNextBits(input, ref inputOffset, ref bitPool, ref bitsLeft, 8);
                
                bytesOutput++;
            }
        }
        
        return result;
    }
    
    private static FileEntry CreateFileEntry(string fileName, ulong fileOffset, long fileOffsetPos, string tocName, string fileType, bool encrypted)
    {
        return new FileEntry
        {
            FileName = fileName,
            FileOffset = fileOffset,
            FileOffsetPos = fileOffsetPos,
            FileOffsetType = typeof(ulong),
            TocName = tocName,
            FileType = fileType,
            Encrypted = encrypted
        };
    }
    
    public void UpdateFileEntry(FileEntry fileEntry)
    {
        if (fileEntry.FileType is not ("FILE" or "HDR"))
            return;
        
        var updateMe = fileEntry.TocName switch
        {
            "CPK" => _cpkPacket,
            "TOC" => _tocPacket,
            "ITOC" => _itocPacket,
            "ETOC" => _etocPacket,
            _ => throw new Exception("I need to implement this TOC!")
        };
        
        // Update ExtractSize
        if (fileEntry.ExtractSizePos > 0)
            UpdateValue(updateMe, fileEntry.ExtractSizePos, fileEntry.ExtractSize, fileEntry.ExtractSizeType!);
        
        // Update FileSize
        if (fileEntry.FileSizePos > 0)
            UpdateValue(updateMe, fileEntry.FileSizePos, fileEntry.FileSize, fileEntry.FileSizeType!);
        
        // Update FileOffset
        if (fileEntry.FileOffsetPos > 0)
        {
            var finalOffset = fileEntry.FileOffset - (ulong)(fileEntry.TocName == "TOC" ? 0x800 : 0);
            
            UpdateValue(updateMe, fileEntry.FileOffsetPos, finalOffset, fileEntry.FileOffsetType!);
        }
        
        switch (fileEntry.TocName)
        {
            case "CPK":
                updateMe = _cpkPacket;
                
                break;
            case "TOC":
                _tocPacket = updateMe;
                
                break;
            case "ITOC":
                _itocPacket = updateMe;
                
                break;
            case "ETOC":
                updateMe = _etocPacket;
                
                break;
            default:
                throw new Exception("I need to implement this TOC!");
        }
    }
    
    public void WriteCpk(EndianWriter writer)
    {
        WritePacket(writer, "CPK ", 0, _cpkPacket);
        
        writer.Seek(0x800 - 6, SeekOrigin.Begin);
        writer.Write("(c)CRI"u8);
    }
    
    public void WriteToc(EndianWriter writer)
    {
        WritePacket(writer, "TOC ", _tocOffset, _tocPacket);
    }
    
    public void WriteItoc(EndianWriter writer)
    {
        WritePacket(writer, "ITOC", _itocOffset, _itocPacket);
    }
    
    public void WriteEtoc(EndianWriter writer)
    {
        WritePacket(writer, "ETOC", _etocOffset, _etocPacket);
    }
    
    public void WriteGtoc(EndianWriter writer)
    {
        WritePacket(writer, "GTOC", _gtocOffset, _gtocPacket);
    }
    
    private static void WritePacket(EndianWriter writer, string id, ulong position, Memory<byte> packet)
    {
        if (position == 0xffffffffffffffff)
            return;
        
        writer.Seek((long)position, SeekOrigin.Begin);
        
        DecryptUtf(packet);
        
        writer.Write(Encoding.ASCII.GetBytes(id));
        writer.Write(0);
        writer.Write((ulong)packet.Length);
        writer.Write(packet.Span);
    }
    
    private static void UpdateValue(Memory<byte> packet, long pos, ulong value, Type type)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(pos, 0);
        
        using var writer = new EndianWriter(packet);
        
        writer.Seek((int)pos, SeekOrigin.Begin);
        
        if (type == typeof(byte))
        {
            writer.Write((byte)value);
        }
        else if (type == typeof(ushort))
        {
            writer.Write((ushort)value);
        }
        else if (type == typeof(uint))
        {
            writer.Write((uint)value);
        }
        else if (type == typeof(ulong))
        {
            writer.Write(value);
        }
        else if (type == typeof(float))
        {
            writer.Write((float)value);
        }
    }
    
    private void ReadUtfData(EndianReader reader)
    {
        _isUtfEncrypted = false;
        reader.IsLittleEndian = true;
        
        //_unk1 = reader.ReadInt32();
        _ = reader.ReadInt32();
        _utfSize = reader.ReadInt64();
        _utfPacket = reader.ReadBytes((int)_utfSize);
        
        if (!_utfPacket.Span[..4].SequenceEqual(UtfHeader))
        {
            DecryptUtf(_utfPacket);
            _isUtfEncrypted = true;
        }
        
        reader.IsLittleEndian = false;
    }
    
    private bool ReadToc(EndianReader reader, ulong tocOffset, ulong contentOffset)
    {
        var addOffset = Math.Min(tocOffset, contentOffset);
        
        reader.Seek((long)tocOffset, SeekOrigin.Begin);
        
        if (Tools.ReadCString(reader, 4) != "TOC ")
        {
            return false;
        }
        
        ReadUtfData(reader);
        
        // Store Encrypted TOC
        _tocPacket = _utfPacket;
        
        // Dump TOC
        var tocEntry = FileTable.Single(x => x.FileName == "TOC_HDR");
        
        tocEntry.Encrypted = _isUtfEncrypted;
        tocEntry.FileSize = (ulong)_tocPacket.Length;
        
        _files = new Utf();
        
        if (!_files.ReadUtf(_utfPacket))
        {
            return false;
        }
        
        for (var i = 0; i < _files.Rows!.Length; i++)
        {
            var fileSize = GetColumnData<uint>(_files, i, "FileSize");
            var extractSize = GetColumnData<uint>(_files, i, "ExtractSize");
            var fileOffset = GetColumnData<ulong>(_files, i, "FileOffset");
            
            FileTable.Add(new FileEntry
            {
                TocName = "TOC",
                DirName = GetColumnData<string>(_files, i, "DirName").Value!,
                FileName = GetColumnData<string>(_files, i, "FileName").Value!,
                FileSize = fileSize.Value,
                FileSizePos = fileSize.Position,
                FileSizeType = fileSize.Type!,
                ExtractSize = extractSize.Value,
                ExtractSizePos = extractSize.Position,
                ExtractSizeType = extractSize.Type!,
                FileOffset = fileOffset.Value + addOffset,
                FileOffsetPos = fileOffset.Position,
                FileOffsetType = fileOffset.Type!,
                FileType = "FILE",
                Id = GetColumnData<int>(_files, i, "ID").Value,
                UserString = GetColumnData<string>(_files, i, "UserString").Value!
            });
        }
        
        _files = null;
        
        return true;
    }
    
    private bool ReadEtoc(EndianReader reader, ulong offset)
    {
        reader.Seek((long)offset, SeekOrigin.Begin);
        
        if (Tools.ReadCString(reader, 4) != "ETOC")
        {
            return false;
        }
        
        ReadUtfData(reader);
        
        _etocPacket = _utfPacket;
        
        // Dump ETOC
        var etocEntry = FileTable.Single(x => x.FileName == "ETOC_HDR");
        
        etocEntry.Encrypted = _isUtfEncrypted;
        etocEntry.FileSize = (ulong)_etocPacket.Length;
        
        _files = new Utf();
        
        if (!_files.ReadUtf(_utfPacket))
        {
            return false;
        }
        
        var fileEntries = FileTable.Where(x => x.FileType == "FILE").ToList();
        
        for (var i = 0; i < fileEntries.Count; i++)
        {
            FileTable[i].LocalDir = GetColumnData<string>(_files, i, "LocalDir").Value!;
        }
        
        return true;
    }
    
    private bool ReadItoc(EndianReader reader, ulong itocOffset, ulong contentOffset, ushort align)
    {
        reader.Seek((long)itocOffset, SeekOrigin.Begin);
        
        if (Tools.ReadCString(reader, 4) != "ITOC")
        {
            return false;
        }
        
        ReadUtfData(reader);
        
        _itocPacket = _utfPacket;
        
        // Dump ITOC
        var itocEntry = FileTable.Single(x => x.FileName == "ITOC_HDR");
        
        itocEntry.Encrypted = _isUtfEncrypted;
        itocEntry.FileSize = (ulong)_itocPacket.Length;
        
        _files = new Utf();
        
        if (!_files.ReadUtf(_utfPacket))
        {
            return false;
        }
        
        var dataL = GetColumnData<byte[]>(_files, 0, "DataL");
        var dataH = GetColumnData<byte[]>(_files, 0, "DataH");
        
        var fileSizeL = GetFileSizeInfo<ushort>(dataL);
        var fileSizeH = GetFileSizeInfo<uint>(dataH);
        
        var tempFiles = new List<FileEntry>(fileSizeL.Ids.Count + fileSizeH.Ids.Count);
        
        AddDataInfoToFileTable(fileSizeL, tempFiles);
        AddDataInfoToFileTable(fileSizeH, tempFiles);
        
        tempFiles.Sort(FileEntryIdComparer.Instance);
        
        RecalculateFileOffset(tempFiles, contentOffset, align);
        
        FileTable.AddRange(tempFiles);
        
        _files = null;
        
        return true;
    }
    
    private static bool ReadGtoc(EndianReader reader, ulong offset)
    {
        reader.Seek((long)offset, SeekOrigin.Begin);
        
        if (Tools.ReadCString(reader, 4) != "GTOC")
        {
            return false;
        }
        
        reader.Seek(0xC, SeekOrigin.Begin); // Skip header area
        
        return true;
    }
    
    private static void DecryptUtf(Memory<byte> input)
    {
        var span = input.Span;
        
        var m = 0x0000655F;       // State
        const int t = 0x00004115; // Multiplier
        
        for (var i = 0; i < input.Length; i++)
        {
            var d = span[i]; // Current Byte
            d = (byte)(d ^ (byte)(m & 0xFF));
            span[i] = d;
            m *= t;
        }
    }
    
    private static ushort GenNextBits(byte[] input, ref int offsetP, ref byte bitPoolP, ref int bitsLeftP, int bitCount)
    {
        ushort outBits = 0;
        var numBitsProduced = 0;
        
        while (numBitsProduced < bitCount)
        {
            if (bitsLeftP == 0)
            {
                bitPoolP = input[offsetP];
                bitsLeftP = 8;
                offsetP--;
            }
            
            int bitsThisRound;
            
            if (bitsLeftP > (bitCount - numBitsProduced))
            {
                bitsThisRound = bitCount - numBitsProduced;
            }
            else
            {
                bitsThisRound = bitsLeftP;
            }
            
            outBits <<= bitsThisRound;
            
            outBits |= (ushort)((ushort)(bitPoolP >> (bitsLeftP - bitsThisRound)) & (1 << bitsThisRound) - 1);
            
            bitsLeftP -= bitsThisRound;
            numBitsProduced += bitsThisRound;
        }
        
        return outBits;
    }
    
    private static void AddDataInfoToFileTable<T>(DataInfo<T> dataInfo, List<FileEntry> tempList)
    {
        foreach (var id in dataInfo.Ids)
        {
            var temp = new FileEntry();
            dataInfo.SizeTable.TryGetValue(id, out var sizeData);
            
            ulong fileSize = sizeData.Value switch
            {
                ushort us => us,
                uint ui => ui,
                _ => 0
            };
            
            temp.TocName = "ITOC";
            temp.FileName = id.ToString("D4");
            temp.FileSize = fileSize;
            temp.FileSizePos = sizeData.Position;
            temp.FileSizeType = sizeData.Type!;
            
            if (dataInfo.CSizeTable.TryGetValue(id, out var cSizeData))
            {
                ulong extractSize = cSizeData.Value switch
                {
                    ushort us => us,
                    uint ui => ui,
                    _ => 0
                };
                
                temp.ExtractSize = extractSize;
                temp.ExtractSizePos = cSizeData.Position;
                temp.ExtractSizeType = cSizeData.Type!;
            }
            
            temp.FileOffsetType = typeof(ulong);
            temp.FileType = "FILE";
            temp.Id = id;
            
            tempList.Add(temp);
        }
    }
    
    private static void RecalculateFileOffset(IEnumerable<FileEntry> entries, ulong contentOffset, ushort align)
    {
        foreach (var entry in entries)
        {
            var fileSize = Convert.ToInt32(entry.FileSize);
            
            entry.FileOffset = contentOffset;
            
            if (fileSize % align > 0)
                contentOffset += (ulong)(fileSize + (align - (fileSize % align)));
            else
                contentOffset += (ulong)fileSize;
        }
    }
    
    private static bool TryGetColumnData<T>(Utf utf, int rowIndex, string columnName, out ColumnData<T> columnData)
    {
        var data = GetColumnData<T>(utf, rowIndex, columnName);
        
        if (data.Position == -1)
        {
            columnData = default;
            
            return false;
        }
        
        columnData = data;
        
        return true;
    }
    
    private static ColumnData<T> GetColumnData<T>(Utf utf, int rowIndex, string columnName)
    {
        try
        {
            var columnIndex = utf.Columns!.FindIndex(c => c.Name == columnName);
            
            if (columnIndex == -1)
            {
                return new ColumnData<T>(default!, -1, null);
            }
            
            var row = utf.Rows![rowIndex][columnIndex];
            
            var cell = row.GetValue();
            var position = row.Position;
            var type = row.GetType();
            
            if (cell is T value)
                return new ColumnData<T>(value, position, type);
            
            if (type == null)
            {
                value = default!;
                position = -1;
            }
            else
            {
                value = (T)Convert.ChangeType(cell, typeof(T))!;
            }
            
            return new ColumnData<T>(value, position, type);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        
        return new ColumnData<T>(default!, -1, null);
    }
    
    private static DataInfo<T> GetFileSizeInfo<T>(ColumnData<byte[]> inputData) where T : INumber<T>
    {
        var result = new DataInfo<T>();
        
        if (inputData.Value == null)
            return result;
        
        var utfData = new Utf();
        utfData.ReadUtf(inputData.Value);
        
        for (var i = 0; i < utfData.Rows!.Length; i++)
        {
            var id = GetColumnData<ushort>(utfData, i, "ID").Value;
            
            var fileSize = GetColumnData<T>(utfData, i, "FileSize");
            
            fileSize.Position += inputData.Position;
            
            result.SizeTable.Add(id, fileSize);
            
            var extractSize = GetColumnData<T>(utfData, i, "ExtractSize");
            
            if (extractSize.Value != default)
            {
                extractSize.Position += inputData.Position;
                
                result.CSizeTable.Add(id, extractSize);
            }
            
            result.Ids.Add(id);
        }
        
        return result;
    }
}

internal record struct ColumnData<T>(T? Value, long Position, Type? Type);

internal record struct DataInfo<T>()
{
    public List<int> Ids { get; } = [];
    public Dictionary<int, ColumnData<T>> SizeTable { get; } = [];
    public Dictionary<int, ColumnData<T>> CSizeTable { get; } = [];
}
