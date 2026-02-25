using System.Reflection;
using ShinRyuModManager.Attributes;
using ShinRyuModManager.ModLoadOrder.Mods;

namespace ShinRyuModManager.Extensions;

public static class ProfileExtensions
{
    public static ProfileMask ToMask(this Profile profile)
    {
        return (ProfileMask)(1 << (int)profile);
    }
    
    public static bool AppliesTo(this ProfileMask mask, Profile profile)
    {
        return mask.HasFlag(profile.ToMask());
    }
    
    public static string GetDescription(this Profile profile)
    {
        var field = profile.GetType().GetField(profile.ToString());
        var attr = field?.GetCustomAttribute<DescriptionAttribute>();
        
        return attr?.Name ?? profile.ToString();
    }
}
