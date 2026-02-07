// -------------------------------------------------------
// © Kaplas. Licensed under MIT. See LICENSE for details.
// -------------------------------------------------------

using Yarhl.FileFormat;
using Yarhl.IO;

namespace ParLibrary;
    
/// <summary>
/// Represents a file stored in a PAR archive.
/// </summary>
public class ParFile : BinaryFormat, IConverter<BinaryFormat, ParFile> {
    /// <summary>
    /// Initializes a new instance of the <see cref="ParFile"/> class.
    /// </summary>
    public ParFile() {
        DecompressedSize = 0;
        FileDate = DateTime.Now;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ParFile"/> class.
    /// </summary>
    /// <param name="stream">The data stream.</param>
    public ParFile(DataStream stream) : base(stream) {
        ArgumentNullException.ThrowIfNull(stream);

        DecompressedSize = (uint)stream.Length;
        FileDate = DateTime.Now;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ParFile"/> class.
    /// </summary>
    /// <param name="stream">The base stream.</param>
    /// <param name="offset">Start offset.</param>
    /// <param name="length">Data length.</param>
    public ParFile(DataStream stream, long offset, long length) : base(stream, offset, length) {
        DecompressedSize = (uint)length;
        FileDate = DateTime.Now;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the file can be compressed.
    /// </summary>
    public bool CanBeCompressed { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the file is compressed.
    /// </summary>
    public bool IsCompressed { get; init; }
    
    /// <summary>
    /// Gets or sets the file size (decompressed).
    /// </summary>
    public uint DecompressedSize { get; init; }

    /// <summary>
    /// Gets or sets the file attributes.
    /// </summary>
    public int Attributes { get; init; } = 0x00000020;
    
    /// <summary>
    /// Gets or sets the file date (as ulong).
    /// </summary>
    public ulong Timestamp { get; init; }

    /// <summary>
    /// Gets or sets the file date (as DateTime).
    /// </summary>
    public DateTime FileDate {
        get => DateTime.UnixEpoch.AddSeconds(Timestamp);

        private init => Timestamp = (ulong)(value - DateTime.UnixEpoch).TotalSeconds;
    }
    
    /// <inheritdoc/>
    public ParFile Convert(BinaryFormat source) {
        ArgumentNullException.ThrowIfNull(source);

        return new ParFile(source.Stream, 0, source.Stream.Length);
    }
}
