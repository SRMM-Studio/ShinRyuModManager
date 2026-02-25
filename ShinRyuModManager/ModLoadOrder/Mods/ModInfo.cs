using CommunityToolkit.Mvvm.ComponentModel;
using ShinRyuModManager.Extensions;

namespace ShinRyuModManager.ModLoadOrder.Mods;

public sealed partial class ModInfo : ObservableObject, IEquatable<ModInfo>
{
    [ObservableProperty] private string _name;
    
    public ProfileMask EnabledProfiles { get; set; }
    
    public bool Enabled
    {
        get => EnabledProfiles.AppliesTo(Program.ActiveProfile);
        set
        {
            if (value)
            {
                EnabledProfiles |= Program.ActiveProfile.ToMask();
            }
            else
            {
                EnabledProfiles &= ~Program.ActiveProfile.ToMask();
            }
        }
    }
    
    public ModInfo(string name, ProfileMask enabledMask = ProfileMask.All)
    {
        Name = name;
        EnabledProfiles = enabledMask;
    }
    
    public void ToggleEnabled()
    {
        Enabled = !Enabled;
    }
    
    public bool Equals(ModInfo other)
    {
        if (other is null)
            return false;
        
        if (ReferenceEquals(this, other))
            return true;
        
        return Name == other.Name;
    }
    
    public override bool Equals(object obj)
    {
        if (obj is null)
            return false;
        
        if (ReferenceEquals(this, obj))
            return true;
        
        if (obj.GetType() != GetType())
            return false;
        
        return Equals((ModInfo)obj);
    }
    
    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}
