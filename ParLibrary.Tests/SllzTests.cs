using System.Text;
using ParLibrary.Sllz;
using Yarhl.FileFormat;
using Yarhl.IO;

namespace ParLibrary.Tests;

[TestClass]
public class SllzTests {
    private const string SAMPLE_TEXT = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed pulvinar leo nec pulvinar pellentesque. Sed id dui et nisl tincidunt dignissim. Suspendisse ullamcorper eget ipsum et vehicula. Maecenas scelerisque dapibus rutrum. Suspendisse tincidunt dictum maximus. Ut rhoncus, lorem scelerisque euismod rhoncus, nunc augue egestas magna, ac mattis elit sapien eu erat. Pellentesque auctor in erat id molestie. Nam vehicula odio eget ipsum porta euismod. Donec eget placerat turpis. Aliquam erat volutpat. Etiam faucibus ligula sit amet ante tincidunt, sit amet efficitur justo lobortis. Nam volutpat augue at purus viverra tincidunt. Nam sapien eros, fringilla sollicitudin semper sed, bibendum eu nisl.\nPellentesque mattis, sem a placerat dictum, risus nisl faucibus odio, quis blandit est erat sed tellus. Mauris iaculis odio sed leo suscipit ultrices. Nunc leo purus, tempor at lobortis vel, molestie pellentesque dui. Sed ac nunc et metus placerat euismod non vel nisl. Proin fringilla viverra aliquet. Pellentesque scelerisque fermentum eleifend. Quisque rutrum orci nulla, sed ullamcorper nulla porttitor nec. Fusce elementum a nulla et ultricies. Pellentesque bibendum blandit leo nec gravida. Donec egestas vitae tellus id auctor.\nMorbi ultrices maximus mattis. Aliquam lobortis at lectus ut scelerisque. Morbi sem tortor, blandit non risus vel, commodo interdum neque. Fusce eu hendrerit mauris, in venenatis est. Vivamus vulputate placerat justo at condimentum. Proin porttitor ac velit quis pretium. Proin vitae lacus sit amet felis aliquam tempor. Fusce dignissim mi id dui imperdiet, nec commodo neque malesuada. Nullam tincidunt augue pellentesque aliquam fringilla. Sed placerat, nibh id fermentum volutpat, ligula felis sagittis lectus, vel semper ipsum arcu at ipsum. Etiam posuere tincidunt augue non gravida. Vivamus posuere posuere dui, a semper urna imperdiet a. Proin quis odio condimentum, pulvinar ipsum vitae, eleifend lorem. Praesent vel iaculis nisi. Aliquam imperdiet eleifend nisi a imperdiet. Mauris vel aliquam quam, ac imperdiet elit.\nVivamus semper odio nec scelerisque porttitor. Quisque ut pulvinar tortor, et consectetur sapien. Fusce neque nulla, laoreet tincidunt risus sit amet, dignissim condimentum enim. Donec dui libero, pulvinar a nibh eget, varius convallis diam. Donec rhoncus elit vel nibh varius, at rhoncus justo consectetur. Pellentesque congue feugiat pulvinar. Donec aliquet sapien ut egestas feugiat. Praesent rhoncus libero libero, non auctor tellus fermentum at. Nunc mollis ornare lacus, et sodales enim rhoncus non. Curabitur vitae dui finibus, varius ex quis, pellentesque ipsum. Etiam metus diam, porttitor in odio vel, suscipit malesuada odio. Nullam eget leo tortor. Duis id posuere augue. Vivamus consequat libero ipsum, nec efficitur ante maximus ullamcorper. Praesent consequat tristique lectus, in aliquam libero hendrerit ac.\nDuis nec felis risus. Pellentesque sit amet massa id lorem ullamcorper rhoncus dignissim pellentesque felis. Suspendisse vitae magna ut ex sodales porta. Proin facilisis augue consectetur ante feugiat vestibulum. Maecenas imperdiet enim nunc, a tincidunt eros luctus et. Nunc gravida, odio eget cursus efficitur, nulla mauris rhoncus mi, rutrum ullamcorper ante ante vitae elit. Quisque vel lacinia justo, in vestibulum nisi. Nam quis maximus ligula, eget ornare mi. Etiam porttitor orci non nulla vestibulum, vitae eleifend nulla pellentesque. Vestibulum eu orci quam. Nullam id tellus viverra, finibus purus et, iaculis diam. Sed ut volutpat eros. In vitae ultrices nibh.";
    
    [TestMethod]
    [DataRow((byte)0x01)]
    [DataRow((byte)0x02)]
    public void TestSllz(byte compressionVersion) {
        var buffer = Encoding.ASCII.GetBytes(SAMPLE_TEXT);
        var dataStream = DataStreamFactory.FromArray(buffer, 0, buffer.Length);
        
        using var binaryFormat = new ParFile(dataStream);
        
        var parameters = new CompressorParameters {
            Endianness = 0x00,
            Version = compressionVersion
        };
        
        var compressedBinaryFormat = (ParFile)ConvertFormat.With(typeof(Compressor), binaryFormat, parameters);
        var decompressedBinaryFormat = (ParFile)ConvertFormat.With(typeof(Decompressor), compressedBinaryFormat);
        
        Assert.IsLessThan(decompressedBinaryFormat.Stream.Length, compressedBinaryFormat.Stream.Length);
        Assert.IsTrue(dataStream.Compare(decompressedBinaryFormat.Stream));
    }
}
