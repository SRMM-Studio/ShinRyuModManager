// -------------------------------------------------------
// © Kaplas. Licensed under MIT. See LICENSE for details.
// -------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Text;
using Yarhl.FileFormat;
using Yarhl.FileSystem;
using Yarhl.IO;

namespace ParLibrary.Converter;
    
/// <summary>
/// Converter from BinaryFormat to ParArchive.
/// </summary>
public class ParArchiveReader : IConverter<BinaryFormat, NodeContainerFormat> {
    private ParArchiveReaderParameters _parameters = new ParArchiveReaderParameters {
        Recursive = false,
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="ParArchiveReader"/> class.
    /// </summary>
    public ParArchiveReader() {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParArchiveReader"/> class.
    /// </summary>
    /// <param name="parameters">The parameters.</param>
    public ParArchiveReader(ParArchiveReaderParameters parameters) : this() {
        _parameters = parameters;
    }
    
    public void Initialize(ParArchiveReaderParameters parameters) {
        _parameters = parameters;
    }
    
    /// <inheritdoc/>
    public NodeContainerFormat Convert(BinaryFormat source) {
        ArgumentNullException.ThrowIfNull(source);

        source.Stream.Position = 0;
        
        var result = new NodeContainerFormat();
        
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var reader = new DataReader(source.Stream) {
            DefaultEncoding = Encoding.GetEncoding(1252),
            Endianness = EndiannessMode.BigEndian,
        };
        
        var magicId = reader.ReadString(4);
        
        if (magicId == "SLLZ") {
            var subStream = new DataStream(source.Stream, 0, source.Stream.Length);
            var compressed = new ParFile(subStream);
            
            source = (ParFile)ConvertFormat.With(typeof(Sllz.Decompressor), compressed);
            source.Stream.Position = 0;
            
            reader = new DataReader(source.Stream) {
                DefaultEncoding = Encoding.GetEncoding(1252),
                Endianness = EndiannessMode.BigEndian,
            };
            
            magicId = reader.ReadString(4);
        }
        
        if (magicId != "PARC") {
            throw new FormatException("PARC: Bad magic Id.");
        }
        
        result.Root.Tags["PlatformId"] = reader.ReadByte();
        
        var endianness = reader.ReadByte();
        
        result.Root.Tags["Endianness"] = endianness;
        result.Root.Tags["SizeExtended"] = reader.ReadByte();
        result.Root.Tags["Relocated"] = reader.ReadByte();
        
        if (endianness == 0x00) {
            reader.Endianness = EndiannessMode.LittleEndian;
        }
        
        result.Root.Tags["Version"] = reader.ReadInt32();
        result.Root.Tags["DataSize"] = reader.ReadInt32();
        
        var totalFolderCount = reader.ReadInt32();
        var folderInfoOffset = reader.ReadInt32();
        var totalFileCount = reader.ReadInt32();
        var fileInfoOffset = reader.ReadInt32();
        
        var folderNames = new string[totalFolderCount];
        
        for (var i = 0; i < totalFolderCount; i++) {
            folderNames[i] = reader.ReadString(0x40).TrimEnd('\0');
            
            if (folderNames[i].Length < 1) {
                folderNames[i] = ".";
            }
        }
        
        var fileNames = new string[totalFileCount];
        
        for (var i = 0; i < totalFileCount; i++) {
            fileNames[i] = reader.ReadString(0x40).TrimEnd('\0');
        }
        
        reader.Stream.Seek(folderInfoOffset);
        
        var folders = new Node[totalFolderCount];
        
        for (var i = 0; i < totalFolderCount; i++) {
            folders[i] = new Node(folderNames[i], new NodeContainerFormat()) {
                Tags = {
                    ["FolderCount"] = reader.ReadInt32(),
                    ["FirstFolderIndex"] = reader.ReadInt32(),
                    ["FileCount"] = reader.ReadInt32(),
                    ["FirstFileIndex"] = reader.ReadInt32(),
                    ["Attributes"] = reader.ReadInt32(),
                    ["Unused1"] = reader.ReadInt32(),
                    ["Unused2"] = reader.ReadInt32(),
                    ["Unused3"] = reader.ReadInt32(),
                },
            };
        }
        
        reader.Stream.Seek(fileInfoOffset);
        
        var files = new Node[totalFileCount];
        
        for (var i = 0; i < totalFileCount; i++) {
            var compressionFlag = reader.ReadUInt32();
            var size = reader.ReadUInt32();
            var compressedSize = reader.ReadUInt32();
            var baseOffset = reader.ReadUInt32();
            var attributes = reader.ReadInt32();
            var extendedOffset = reader.ReadUInt32();
            var timestamp = reader.ReadUInt64();
            
            var offset = ((long)extendedOffset << 32) | baseOffset;
            
            offset &= 0x00FFFFFFFFFFFFFF;
            
            var file = new ParFile(source.Stream, offset, compressedSize) {
                CanBeCompressed = false, // Don't try to compress if the original was not compressed.
                IsCompressed = compressionFlag == 0x80000000,
                DecompressedSize = size,
                Attributes = attributes,
                Timestamp = timestamp,
            };
            
            files[i] = new Node(fileNames[i], file) {
                Tags = { ["Timestamp"] = timestamp, },
            };
        }
        
        BuildTree(folders[0], folders, files, _parameters);
        
        result.Root.Add(folders[0]);
        
        return result;
    }
    
    private static void BuildTree(Node node, IReadOnlyList<Node> folders, IReadOnlyList<Node> files, ParArchiveReaderParameters parameters) {
        int firstFolderIndex = node.Tags["FirstFolderIndex"];
        int folderCount = node.Tags["FolderCount"];
        
        for (var i = firstFolderIndex; i < firstFolderIndex + folderCount; i++) {
            node.Add(folders[i]);
            
            BuildTree(folders[i], folders, files, parameters);
        }
        
        int firstFileIndex = node.Tags["FirstFileIndex"];
        int fileCount = node.Tags["FileCount"];
        
        for (var i = firstFileIndex; i < firstFileIndex + fileCount; i++) {
            if (parameters.Recursive &&
                files[i].Name.EndsWith(".par", StringComparison.InvariantCultureIgnoreCase)) {
                files[i].TransformWith(typeof(ParArchiveReader), parameters);
            }
            
            node.Add(files[i]);
        }
    }
}
