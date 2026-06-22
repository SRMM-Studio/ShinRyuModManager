namespace ShinRyuModManager.ModLoadOrder.Mods;

[Flags]
public enum ProfileMask : byte
{
    Profile1 = 1 << 0,
    Profile2 = 1 << 1,
    Profile3 = 1 << 2,
    Profile4 = 1 << 3,
    Profile5 = 1 << 4,
    Profile6 = 1 << 5,
    Profile7 = 1 << 6,
    Profile8 = 1 << 7,
    All = Profile1 | Profile2 | Profile3 | Profile4 | Profile5 | Profile6 | Profile7 | Profile8
}
