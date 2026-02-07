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
public class Compressor : IConverter<ParFile, ParFile> {
    private const int MAX_WINDOW_SIZE = 4096;
    private const int MAX_ENCODED_LENGTH = 18;
    
    private CompressorParameters _compressorParameters;
    /// <summary>
    /// Initializes a new instance of the <see cref="Compressor"/> class.
    /// </summary>
    public Compressor() { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Compressor"/> class.
    /// </summary>
    /// <param name="parameters">Compressor configuration.</param>
    public Compressor(CompressorParameters parameters) {
        _compressorParameters = parameters;
    }
    
    /// <summary>
    /// Initializes the compressor parameters.
    /// </summary>
    /// <param name="parameters">Compressor configuration.</param>
    public void Initialize(CompressorParameters parameters) {
        _compressorParameters = parameters;
    }
    
    /// <summary>Compresses a file with SLLZ.</summary>
    /// <returns>The compressed file.</returns>
    /// <param name="source">Source file to compress.</param>
    public ParFile Convert(ParFile source) {
        ArgumentNullException.ThrowIfNull(source);

        source.Stream.Seek(0);
        
        var outputDataStream = Compress(source.Stream, _compressorParameters);
        var result = new ParFile(outputDataStream) {
            CanBeCompressed = false,
            IsCompressed = true,
            DecompressedSize = source.DecompressedSize,
            Attributes = source.Attributes,
            Timestamp = source.Timestamp,
        };
        
        return result;
    }
    
    private static DataStream Compress(DataStream inputDataStream, CompressorParameters parameters) {
        var outputDataStream = DataStreamFactory.FromMemory();
        var writer = new DataWriter(outputDataStream) {
            DefaultEncoding = Encoding.ASCII,
        };
        
        parameters ??= new CompressorParameters {
            Version = 0x01,
            Endianness = 0x00,
        };
        
        DataStream compressedDataStream;
        
        switch (parameters.Version) {
            case 1:
                try {
                    compressedDataStream = CompressV1(inputDataStream);
                } catch (SllzCompressorException) {
                    compressedDataStream = inputDataStream;
                }
                
                break;
            
            case 2 when inputDataStream.Length < 0x1B:
                throw new FormatException("SLLZv2: Input size must more than 0x1A.");
            
            case 2:
                compressedDataStream = CompressV2(inputDataStream);
                
                break;
            
            default:
                throw new FormatException($"SLLZ: Unknown compression version {parameters.Version}.");
        }
        
        if (compressedDataStream == inputDataStream) {
            return inputDataStream;
        }
        
        writer.Endianness = parameters.Endianness == 0 ? EndiannessMode.LittleEndian : EndiannessMode.BigEndian;
        writer.Write("SLLZ", false);
        writer.Write(parameters.Endianness);
        writer.Write(parameters.Version);
        writer.Write((ushort)0x10); // Header size
        writer.Write((int)inputDataStream.Length);
        
        var currentPos = writer.Stream.Position;
        
        writer.Write(0x00000000); // Compressed size
        
        compressedDataStream.WriteTo(outputDataStream);
        
        writer.Stream.Seek(currentPos);
        writer.Write((int)(compressedDataStream.Length + 0x10)); // data + header
        
        compressedDataStream.Dispose();
        
        return outputDataStream;
    }
    
    private static DataStream CompressV1(DataStream inputDataStream) {
        // It's easier to implement working with a byte array.
        var inputData = new byte[inputDataStream.Length];
        
        inputDataStream.ReadExactly(inputData, 0, inputData.Length);
        
        var outputSize = (uint)inputData.Length + 2048;
        var outputData = new byte[outputSize];
        uint inputPosition = 0;
        uint outputPosition = 0;
        byte currentFlag = 0x00;
        var bitCount = 0;
        long flagPosition = outputPosition;
        
        outputData[flagPosition] = 0x00;
        outputPosition++;
        
        ThrowIfCompressedIsLarger(outputPosition, outputSize);
        
        while (inputPosition < inputData.Length) {
            var windowSize = Math.Min(inputPosition, MAX_WINDOW_SIZE);
            var maxOffsetLength = Math.Min((uint)(inputData.Length - inputPosition), MAX_ENCODED_LENGTH);
            
            var match = FindMatch(inputData, inputPosition, windowSize, maxOffsetLength);
            
            if (match == null) {
                // currentFlag |= (byte)(0 << (7 - bitCount)); // It's zero
                bitCount++;
                
                if (bitCount == 0x08) {
                    outputData[flagPosition] = currentFlag;
                    
                    currentFlag = 0x00;
                    bitCount = 0x00;
                    flagPosition = outputPosition;
                    outputData[flagPosition] = 0x00;
                    outputPosition++;
                    
                    ThrowIfCompressedIsLarger(outputPosition, outputSize);
                }
                
                outputData[outputPosition] = inputData[inputPosition];
                inputPosition++;
                outputPosition++;
                
                ThrowIfCompressedIsLarger(outputPosition, outputSize);
            } else {
                currentFlag |= (byte)(1 << (7 - bitCount));
                bitCount++;
                
                if (bitCount == 0x08) {
                    outputData[flagPosition] = currentFlag;
                    
                    currentFlag = 0x00;
                    bitCount = 0x00;
                    flagPosition = outputPosition;
                    outputData[flagPosition] = 0x00;
                    outputPosition++;
                    
                    ThrowIfCompressedIsLarger(outputPosition, outputSize);
                }
                
                var offset = (short)((match.Item1 - 1) << 4);
                var size = (short)((match.Item2 - 3) & 0x0F);
                var tuple = (short)(offset | size);
                
                outputData[outputPosition] = (byte)tuple;
                outputPosition++;
                
                ThrowIfCompressedIsLarger(outputPosition, outputSize);
                
                outputData[outputPosition] = (byte)(tuple >> 8);
                outputPosition++;

                ThrowIfCompressedIsLarger(outputPosition, outputSize);
                
                inputPosition += match.Item2;
            }
        }
        
        outputData[flagPosition] = currentFlag;
        
        var outputDataStream = DataStreamFactory.FromArray(outputData, 0, (int)outputPosition);
        
        return outputDataStream;
    }
    
    private static DataStream CompressV2(DataStream inputDataStream) {
        var input = new byte[inputDataStream.Length];
        
        inputDataStream.ReadExactly(input, 0, input.Length);
        
        var outputDataStream = DataStreamFactory.FromMemory();
        var writer = new DataWriter(outputDataStream);
        var currentPosition = 0;
        
        while (currentPosition < input.Length) {
            var decompressedChunkSize = Math.Min(input.Length - currentPosition, 0x10000);
            var decompressedData = new byte[decompressedChunkSize];
            
            Array.Copy(input, currentPosition, decompressedData, 0, decompressedChunkSize);
            
            var compressedData = ZlibCompress(decompressedData);
            var compressedDataLength = compressedData.Length + 5;
            
            writer.Write((byte)(compressedDataLength >> 16));
            writer.Write((byte)(compressedDataLength >> 8));
            writer.Write((byte)compressedDataLength);
            
            var temp = decompressedChunkSize - 1;
            
            writer.Write((byte)(temp >> 8));
            writer.Write((byte)temp);
            writer.Write(compressedData);
            
            currentPosition += decompressedChunkSize;
        }
        
        return outputDataStream;
    }
    
    private static Tuple<uint, uint> FindMatch(byte[] inputData, uint inputPosition, uint windowSize, uint maxOffsetLength) {
        ReadOnlySpan<byte> bytes = inputData;
        var data = bytes.Slice((int)(inputPosition - windowSize), (int)windowSize);
        var currentLength = maxOffsetLength;
        
        while (currentLength >= 3) {
            var pattern = bytes.Slice((int)inputPosition, (int)currentLength);
            
            var pos = data.LastIndexOf(pattern);
            
            if (pos >= 0) {
                return new Tuple<uint, uint>((uint)(windowSize - pos), currentLength);
            }
            
            currentLength--;
        }
        
        return null;
    }
    
    private static byte[] ZlibCompress(byte[] decompressedData) {
        using var inputMemoryStream = new MemoryStream(decompressedData);
        using var outputMemoryStream = new MemoryStream();
        using var zlibStream = new ZLibStream(outputMemoryStream, CompressionLevel.SmallestSize);
        
        inputMemoryStream.CopyTo(zlibStream);
        zlibStream.Close();
        
        return outputMemoryStream.ToArray();
    }

    private static void ThrowIfCompressedIsLarger(uint outputPosition, uint outputSize) {
        if (outputPosition >= outputSize) {
            throw new SllzCompressorException("Compressed size is bigger than original size.");
        }
    }
}