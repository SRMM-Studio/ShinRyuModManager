// -------------------------------------------------------
// © Kaplas. Licensed under MIT. See LICENSE for details.
// -------------------------------------------------------

namespace ParLibrary.Sllz;

/// <summary>
/// Parameters for SLLZ compressor.
/// </summary>
public class CompressorParameters {
    /// <summary>
    /// Gets or sets the compression algorithm version.
    /// </summary>
    /// <remarks><para>Valid values are 1 or 2.</para></remarks>
    public byte Version { get; init; }
    
    /// <summary>
    /// Gets or sets the compression algorithm endianness.
    /// </summary>
    /// <remarks><para>Valid values are 0 (Little Endian) or 1 (Big Endian).</para></remarks>
    public byte Endianness { get; init; }
}
