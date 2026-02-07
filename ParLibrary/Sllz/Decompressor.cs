// -------------------------------------------------------
// © Kaplas. Licensed under MIT. See LICENSE for details.
// -------------------------------------------------------

using System.IO.Compression;
using System.Text;
using Yarhl.FileFormat;
using Yarhl.IO;

namespace ParLibrary.Sllz;

/// <summary>
/// Manages SLLZ compression used in Yakuza games.
/// </summary>
public class Decompressor : IConverter<ParFile, ParFile> {
    /// <summary>Decompresses a SLLZ file.</summary>
    /// <returns>The decompressed file.</returns>
    /// <param name="source">Source file to decompress.</param>
    public ParFile Convert(ParFile source) {
        ArgumentNullException.ThrowIfNull(source);

        source.Stream.Position = 0;
        
        var outputDataStream = Decompress(source.Stream);
        
        var result = new ParFile(outputDataStream) {
            CanBeCompressed = true,
            IsCompressed = false,
            DecompressedSize = source.DecompressedSize,
            Attributes = source.Attributes,
            Timestamp = source.Timestamp,
        };
        
        return result;
    }
    
    private static DataStream Decompress(DataStream inputDataStream) {
        var reader = new DataReader(inputDataStream) {
            DefaultEncoding = Encoding.ASCII,
        };
        
        inputDataStream.Seek(0);
        
        var magic = reader.ReadString(4);
        
        if (magic != "SLLZ") {
            throw new FormatException("SLLZ: Bad magic Id.");
        }
        
        var endianness = reader.ReadByte();
        
        reader.Endianness = endianness == 0 ? EndiannessMode.LittleEndian : EndiannessMode.BigEndian;
        
        var version = reader.ReadByte();
        var headerSize = reader.ReadUInt16();
        var decompressedSize = reader.ReadInt32();
        var compressedSize = reader.ReadInt32();
        
        reader.Stream.Seek(headerSize);
        
        return version switch {
            1 => DecompressV1(inputDataStream, compressedSize, decompressedSize),
            2 => DecompressV2(inputDataStream, compressedSize, decompressedSize),
            _ => throw new FormatException($"SLLZ: Unknown compression version {version}.")
        };
    }
    
    private static DataStream DecompressV1(DataStream inputDataStream, int compressedSize, int decompressedSize) {
        var inputData = new byte[compressedSize];
        var outputData = new byte[decompressedSize];
        
        inputDataStream.ReadExactly(inputData, 0, compressedSize - 0x10);
        
        var inputPosition = 0;
        var outputPosition = 0;
        var flag = inputData[inputPosition];
        
        inputPosition++;
        
        var flagCount = 8;
        
        do {
            if ((flag & 0x80) == 0x80) {
                flag = (byte)(flag << 1);
                flagCount--;
                
                if (flagCount == 0) {
                    flag = inputData[inputPosition];
                    inputPosition++;
                    flagCount = 8;
                }
                
                var copyFlags = (ushort)(inputData[inputPosition] | inputData[inputPosition + 1] << 8);
                inputPosition += 2;
                
                var copyDistance = 1 + (copyFlags >> 4);
                var copyCount = 3 + (copyFlags & 0xF);
                
                var i = 0;
                
                do {
                    outputData[outputPosition] = outputData[outputPosition - copyDistance];
                    outputPosition++;
                    i++;
                } while (i < copyCount);
            } else {
                flag = (byte)(flag << 1);
                flagCount--;
                
                if (flagCount == 0) {
                    flag = inputData[inputPosition];
                    inputPosition++;
                    flagCount = 8;
                }
                
                outputData[outputPosition] = inputData[inputPosition];
                inputPosition++;
                outputPosition++;
            }
        } while (outputPosition < decompressedSize);
        
        var outputDataStream = DataStreamFactory.FromArray(outputData, 0, decompressedSize);
        
        return outputDataStream;
    }
    
    private static DataStream DecompressV2(DataStream inputDataStream, int compressedSize, int decompressedSize) {
        var inputData = new byte[compressedSize];
        var outputData = new byte[decompressedSize];
        
        inputDataStream.ReadExactly(inputData, 0, compressedSize - 0x10);
        
        var inputPosition = 0;
        var outputPosition = 0;
        
        while (outputPosition < decompressedSize) {
            var compressedChunkSize = (inputData[inputPosition] << 16) | (inputData[inputPosition + 1] << 8) | inputData[inputPosition + 2];
            var decompressedChunkSize = ((inputData[inputPosition + 3] << 8) | inputData[inputPosition + 4]) + 1;
            var isCompressed = (compressedChunkSize & 0x00800000) == 0x00000000;
            
            if (isCompressed) {
                var decompressedData = ZlibDecompress(inputData, inputPosition + 5, compressedChunkSize - 5);
                
                if (decompressedChunkSize != decompressedData.Length) {
                    throw new FormatException("SLLZ: Wrong decompressed data.");
                }
                
                Array.Copy(decompressedData, 0, outputData, outputPosition, decompressedData.Length);
            } else {
                // The data isn't compressed in this chunk, just copy it
                compressedChunkSize = (int)(compressedChunkSize & 0xFF7FFFFF);
                
                Array.Copy(inputData, inputPosition + 5, outputData, outputPosition, decompressedChunkSize);
            }
            
            inputPosition += compressedChunkSize;
            outputPosition += decompressedChunkSize;
        }
        
        var outputDataStream = DataStreamFactory.FromArray(outputData, 0, decompressedSize);
        
        return outputDataStream;
    }
    
    private static byte[] ZlibDecompress(byte[] compressedData, int index, int count) {
        using var inputMemoryStream = new MemoryStream(compressedData, index, count);
        using var outputMemoryStream = new MemoryStream();
        using var zlibStream = new ZLibStream(inputMemoryStream, CompressionMode.Decompress);
        
        zlibStream.CopyTo(outputMemoryStream);
        
        return outputMemoryStream.ToArray();
    }
}
