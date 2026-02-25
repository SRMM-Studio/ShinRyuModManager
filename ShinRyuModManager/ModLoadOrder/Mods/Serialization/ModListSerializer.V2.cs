using System.Text;
using Utils;

namespace ShinRyuModManager.ModLoadOrder.Mods.Serialization;

// Current binary mod list format (ModList.ml)
public static partial class ModListSerializer
{
    private static List<ModInfo> ReadV2(BinaryReader reader, Profile? profile = null)
    {
        var readProfile = (Profile)reader.ReadByte();
        
        if (profile == null)
        {
            Program.ActiveProfile = readProfile;
        }
        
        var entryCount = reader.ReadUInt16();
        
        var mods = new List<ModInfo>();
        
        for (var i = 0; i < entryCount; i++)
        {
            var entry = ReadEntryV2(reader);
            
            if (!Directory.Exists(GamePath.GetModDirectory(entry.Name)))
                continue;
            
            mods.Add(entry);
        }
        
        return mods;
    }
    
    private static ModInfo ReadEntryV2(BinaryReader reader)
    {
        var mask = (ProfileMask)reader.ReadByte();
        var nameLength = reader.ReadUInt16();
        
        ReadOnlySpan<byte> nameBytes = reader.ReadBytes(nameLength);
        
        var name = Encoding.UTF8.GetString(nameBytes);
        
        return new ModInfo(name, mask);
    }
}
