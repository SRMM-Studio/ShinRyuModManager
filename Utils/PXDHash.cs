using System.Runtime.InteropServices;

namespace Utils;

[StructLayout(LayoutKind.Sequential, Size = 32)]
public struct PXDHash
{
    public ushort Checksum { get; set; }
    
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
    private char[] str; // 0x0002
    
    public void Set(string val)
    {
        Checksum = 0;
        
        if (str == null || str.Length <= 0)
            str = new char[30];
        
        var valChar = val.ToCharArray();
        var len = valChar.Length <= 30 ? valChar.Length : 30;
        
        for (var i = 0; i < len; i++)
        {
            Checksum += (byte)valChar[i];
            str[i] = valChar[i];
        }
    }
    
    public override string ToString()
    {
        return new string(str);
    }
}
