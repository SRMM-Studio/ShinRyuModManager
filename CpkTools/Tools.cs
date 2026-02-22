using System.Text;
using CpkTools.Endian;

namespace CpkTools;

public static class Tools
{
    static Tools()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }
    
    public static string ReadCString(EndianReader reader, int maxLength = -1, long lOffset = -1, Encoding? enc = null)
    {
        enc ??= Encoding.GetEncoding(932);
        
        var max = maxLength == -1 ? 255 : maxLength;
        var fTemp = reader.Position;
        
        if (lOffset >= 0)
        {
            reader.Seek(lOffset, SeekOrigin.Begin);
        }
        
        var length = 0;
        
        while (length < max && reader.ReadByte() != 0)
        {
            length++;
        }
        
        if (maxLength == -1)
            max = length + 1;
        else
            max = maxLength;
        
        var initSeek = lOffset >= 0 ? lOffset : fTemp;
        var returnSeek = lOffset >= 0 ? fTemp : fTemp + max;
        
        reader.Seek(initSeek, SeekOrigin.Begin);
        
        var bytes = reader.ReadBytes(length);
        
        reader.Seek(returnSeek, SeekOrigin.Begin);
        
        return enc.GetString(bytes);
    }
    
    public static void DeleteFileIfExists(string sPath)
    {
        if (File.Exists(sPath))
            File.Delete(sPath);
    }
    
    public static string GetPath(string input)
    {
        return Path.GetDirectoryName(input) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(input);
    }
    
    public static byte[] GetData(EndianReader reader, long offset, int size)
    {
        var backup = reader.Position;
        
        reader.Seek(offset, SeekOrigin.Begin);
        
        var result = reader.ReadBytes(size);
        
        reader.Seek(backup, SeekOrigin.Begin);
        
        return result;
    }
}
