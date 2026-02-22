using System.Text;
using ShinRyuModManager.Exceptions;
using Utils;

namespace ShinRyuModManager.ModLoadOrder.Mods.Serialization;

public static partial class ModListSerializer
{
    private const string SIGNATURE = "SRMM_ML";
    
    private static readonly byte[] SignatureBytes = Encoding.UTF8.GetBytes(SIGNATURE);
    
    public static List<ModInfo> Read(string path, Profile? profile = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        
        if (!File.Exists(path))
            throw new FileNotFoundException("ModList file not found!", path);
        
        switch (path)
        {
            case Constants.TXT_OLD:
                return ReadV0(path);
            case Constants.TXT:
                return ReadV1(path);
        }
        
        using var fs = File.OpenRead(path);
        using var reader = new BinaryReader(fs, Encoding.UTF8, true);
        
        ValidateModList(reader);
        
        var version = reader.ReadByte();
        
        // No V0 or V1 as that's handled before
        return version switch
        {
            2 => ReadV2(reader, profile),
            _ => throw new NotSupportedException()
        };
    }
    
    // Write methods ALWAYS use the newest format. Never need to be in partial classes
    public static void Write(string path, List<ModInfo> mods)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        
        var fileExists = File.Exists(path);
        var mode = fileExists ? FileMode.Truncate : FileMode.CreateNew;
        
        using var fs = new FileStream(path, mode, FileAccess.Write, FileShare.None);
        using var writer = new BinaryWriter(fs, Encoding.UTF8, true);
        
        writer.Write(SignatureBytes);
        writer.Write(Program.CurrentModListVersion);
        writer.Write((byte)Program.ActiveProfile);
        writer.Write((ushort)mods.Count);
        
        foreach (var modInfo in mods)
        {
            WriteEntry(writer, modInfo);
        }
    }
    
    private static void WriteEntry(BinaryWriter writer, ModInfo modInfo)
    {
        ReadOnlySpan<byte> nameBytes = Encoding.UTF8.GetBytes(modInfo.Name);
        
        writer.Write((byte)modInfo.EnabledProfiles);
        writer.Write((ushort)nameBytes.Length);
        writer.Write(nameBytes);
    }
    
    private static void ValidateModList(BinaryReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        
        reader.BaseStream.Seek(0, SeekOrigin.Begin);
        
        var sigBytes = reader.ReadBytes(SIGNATURE.Length);
        
        if (!sigBytes.SequenceEqual(SignatureBytes))
        {
            throw new ModListFileLoadException("File signature doesn't match!");
        }
    }
}
