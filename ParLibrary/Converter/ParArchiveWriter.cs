// -------------------------------------------------------
// © Kaplas. Licensed under MIT. See LICENSE for details.
// -------------------------------------------------------

using System.Text;
using ParLibrary.Sllz;
using Yarhl.FileFormat;
using Yarhl.FileSystem;
using Yarhl.IO;

namespace ParLibrary.Converter;
    
/// <summary>
/// Converter from PAR to BinaryFormat.
/// </summary>
public class ParArchiveWriter : IConverter<NodeContainerFormat, ParFile> {
    private ParArchiveWriterParameters _parameters = new() {
        CompressorVersion = 0x01,
        IncludeDots = false,
    };
    
    /// <summary>
    /// Represents the method that handles a Node event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    public delegate void NodeEventHandler(Node sender);
    
    /// <summary>
    /// Occurs before the nested PAR file is created.
    /// </summary>
    public static event NodeEventHandler NestedParCreating;
    
    /// <summary>
    /// Occurs after the nested PAR file is created.
    /// </summary>
    public static event NodeEventHandler NestedParCreated;
    
    /// <summary>
    /// Occurs before the file is compressed.
    /// </summary>
    public static event NodeEventHandler FileCompressing;
    
    /// <summary>
    /// Occurs after the file is compressed.
    /// </summary>
    public static event NodeEventHandler FileCompressed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParArchiveWriter"/> class.
    /// </summary>
    public ParArchiveWriter() {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ParArchiveWriter"/> class.
    /// </summary>
    /// <param name="parameters">The parameters.</param>
    public ParArchiveWriter(ParArchiveWriterParameters parameters) : this() {
        _parameters = parameters;
    }
    
    public void Initialize(ParArchiveWriterParameters parameters) {
        _parameters = parameters;
    }
    
    /// <summary>
    /// Converts a PAR format into binary.
    /// </summary>
    /// <param name="source">The par.</param>
    /// <returns>The BinaryFormat.</returns>
    public ParFile Convert(NodeContainerFormat source) {
        ArgumentNullException.ThrowIfNull(source);
        
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var dataStream = string.IsNullOrEmpty(_parameters.OutputPath)
            ? DataStreamFactory.FromMemory()
            : DataStreamFactory.FromFile(_parameters.OutputPath, FileOpenMode.Write);
        
        var writer = new DataWriter(dataStream) {
            DefaultEncoding = Encoding.GetEncoding(1252),
            Endianness = EndiannessMode.BigEndian,
        };
        
        var folders = new List<Node>();
        var files = new List<Node>();
        
        if (_parameters.IncludeDots) {
            var parFolderRootNode = new Node(".", new NodeContainerFormat());
            source.MoveChildrenTo(parFolderRootNode);
            folders.Add(parFolderRootNode);
        }
        
        GetFoldersAndFiles(source.Root, folders, files, _parameters);
        CompressFiles(files, _parameters.CompressorVersion);
        
        var headerSize = 32 + (64 * folders.Count) + (64 * files.Count);
        var fileTableOffset = headerSize + (folders.Count * 32);
        long dataPosition = fileTableOffset + (files.Count * 32);
        
        dataPosition = Align(dataPosition, 2048);
        
        writer.Write("PARC", 4, false);
        
        if (source.Root.Tags.TryGetValue("PlatformId", out var platformId)) {
            writer.Write((byte)platformId);
        } else {
            writer.Write((byte)0x02);
        }
        
        if (source.Root.Tags.TryGetValue("Endianness", out var rootTag)) {
            var endianness = (byte)rootTag;
            writer.Write(endianness);
            writer.Endianness = endianness == 0x00 ? EndiannessMode.LittleEndian : EndiannessMode.BigEndian;
        } else {
            writer.Write((byte)0x01);
        }
        
        writer.Write((ushort)0x0000); // extended size and relocated
        
        if (source.Root.Tags.TryGetValue("Version", out var version)) {
            writer.Write((int)version);
        } else {
            writer.Write(0x00020001);
        }
        
        writer.Write(0x00000000); // data size
        
        writer.Write(folders.Count);
        writer.Write(headerSize);
        writer.Write(files.Count);
        writer.Write(fileTableOffset);
        
        WriteNames(writer, folders);
        WriteNames(writer, files);
        
        WriteFolders(writer, folders);
        WriteFiles(writer, files, dataPosition, _parameters);
        
        dataStream.Seek(0, SeekOrigin.End);
        writer.WritePadding(0, 2048);
        
        var result = new ParFile(dataStream) {
            CanBeCompressed = false,
        };
        
        return result;
    }
    
    private static void GetFoldersAndFiles(Node root, List<Node> folders, List<Node> files, ParArchiveWriterParameters parameters) {
        var folderIndex = folders.Count;
        var fileIndex = 0;
        
        var queue = new Queue<Node>();
        
        queue.Enqueue(root);
        
        while (queue.Count != 0) {
            var folder = queue.Dequeue();
            
            folder.Tags["FirstFolderIndex"] = folderIndex;
            folder.Tags["FolderCount"] = 0;
            folder.Tags["FirstFileIndex"] = fileIndex;
            folder.Tags["FileCount"] = 0;
            folder.Tags["Attributes"] = 0x00000010;
            folder.Tags["Unused1"] = 0x00000000;
            folder.Tags["Unused2"] = 0x00000000;
            folder.Tags["Unused3"] = 0x00000000;
            
            foreach (var child in folder.Children) {
                if (child.IsContainer) {
                    if (child.Name.EndsWith(".par", StringComparison.InvariantCultureIgnoreCase)) {
                        NestedParCreating?.Invoke(child);
                        
                        child.TransformWith(new ParArchiveWriter(
                            new ParArchiveWriterParameters {
                                CompressorVersion = parameters.CompressorVersion,
                                IncludeDots = parameters.IncludeDots,
                            }));
                        
                        NestedParCreated?.Invoke(child);
                        
                        files.Add(child);
                        fileIndex++;
                        folder.Tags["FileCount"]++;
                    } else {
                        folders.Add(child);
                        folderIndex++;
                        folder.Tags["FolderCount"]++;
                        
                        queue.Enqueue(child);
                    }
                } else {
                    if (child.Format is not ParFile) {
                        child.TransformWith<ParFile>();
                    }
                    
                    files.Add(child);
                    fileIndex++;
                    folder.Tags["FileCount"]++;
                }
            }
        }
    }
    
    private static void CompressFiles(IEnumerable<Node> files, int compressorVersion) {
        var compressorParameters = new CompressorParameters {
            Endianness = 0x00,
            Version = (byte)compressorVersion,
        };
        
        Parallel.ForEach(files, node => {
            var parFile = node.GetFormatAs<ParFile>();
            
            if (parFile == null || !parFile.CanBeCompressed || compressorVersion == 0x00 || parFile.Stream.Length == 0) {
                return;
            }
            
            FileCompressing?.Invoke(node);
            
            var compressed = (ParFile)ConvertFormat.With(typeof(Compressor), parFile, compressorParameters);
            
            var diff = parFile.Stream.Length - compressed.Stream.Length;
            
            if (diff >= 0 && (parFile.Stream.Length < 2048 || diff >= 2048)) {
                node.ChangeFormat(compressed);
            }
            
            FileCompressed?.Invoke(node);
        });
    }
    
    private static long Align(long position, int align) {
        if (position % align == 0) {
            return position;
        }
        
        var padding = align + (-position % align);
        
        return position + padding;
    }
    
    private static void WriteNames(DataWriter writer, IEnumerable<Node> nodes) {
        foreach (var node in nodes) {
            writer.Write(node.Name, 64, false);
        }
    }
    
    private static void WriteFolders(DataWriter writer, IEnumerable<Node> folders) {
        foreach (var tags in folders.Select(x => x.Tags)) {
            var attributes = 0x00000010;
            
            if (tags.TryGetValue("DirectoryInfo", out var directoryInfo)) {
                DirectoryInfo info = directoryInfo;
                attributes = (int)info.Attributes;
            }
            
            if (tags.TryGetValue("Attributes", out var attrs)) {
                attributes = (int)attrs;
            }
            
            writer.Write((int)tags["FolderCount"]);
            writer.Write((int)tags["FirstFolderIndex"]);
            writer.Write((int)tags["FileCount"]);
            writer.Write((int)tags["FirstFileIndex"]);
            writer.Write(attributes);
            writer.Write(0x00000000);
            writer.Write(0x00000000);
            writer.Write(0x00000000);
        }
    }
    
    private static void WriteFiles(DataWriter writer, IEnumerable<Node> files, long dataPosition, ParArchiveWriterParameters parameters) {
        long blockSize = 0;
        
        foreach (var node in files) {
            var parFile = node.GetFormatAs<ParFile>();
            
            if (parFile == null)
                continue;

            var streamLength = node.Stream!.Length;

            if (streamLength > 2048 || streamLength >= blockSize) {
                blockSize = 2048 + (-streamLength % 2048);
                dataPosition = Align(dataPosition, 2048);
            } else {
                blockSize -= streamLength;
            }

            ulong seconds = 0;
            var attributes = parFile.Attributes;

            if (!parameters.ResetFileDates) {
                var date = parFile.FileDate;
                var baseDate = DateTime.UnixEpoch;
            
                if (node.Tags.TryGetValue("Timestamp", out var timestamp)) {
                    date = baseDate.AddSeconds(timestamp);
                }
            
                if (node.Tags.TryGetValue("FileInfo", out var fileInfo) && fileInfo is FileInfo info) {
                    attributes = HandleAttributes(info);
            
                    date = info.LastWriteTime;
                }
            
                seconds = (ulong)(date - baseDate).TotalSeconds;
            }
            
            writer.Write(parFile.IsCompressed ? 0x80000000 : 0x00000000);
            writer.Write(parFile.DecompressedSize);
            writer.Write((uint)node.Stream.Length);
            writer.Write((uint)dataPosition);
            writer.Write(attributes);
            writer.Write((uint)(dataPosition >> 32));
            writer.Write(seconds);
            
            var currentPos = writer.Stream.Position;
            
            writer.Stream.Seek(0, SeekOrigin.End);
            writer.WriteUntilLength(0, dataPosition);
            node.Stream.WriteTo(writer.Stream);
            dataPosition = writer.Stream.Position;
            writer.Stream.Seek(currentPos);
        }
    }

    // Linux doesn't have `FileAttributes.Archive` and Wine defaults files to have the Archive bit set
    // So follow Wine and assume loose files set as `FileAttributes.Normal` have the Archive bit
    private static int HandleAttributes(FileInfo fileInfo) {
        if (!OperatingSystem.IsLinux())
            return (int)fileInfo.Attributes;

        if (fileInfo.Attributes == FileAttributes.Normal) {
            return (int)FileAttributes.Archive;
        }

        return (int)fileInfo.Attributes;
    }
}