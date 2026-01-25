using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Utils
{
    [StructLayout(LayoutKind.Sequential, Size = 32)]
    public struct PXDHash
    {
        public ushort Checksum; //0x0000
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
        char[] str; //0x0002


        public override string ToString()
        {
            return new string(str);
        }

        public void Set(string val)
        {
            Checksum = 0;

            if (str == null || str.Length <= 0)
                str = new char[30];

            char[] valChar = val.ToCharArray();

            int len = (valChar.Length <= 30 ? valChar.Length : 30);

            for (int i = 0; i < len; i++)
            {
                Checksum += (byte)valChar[i];
                str[i] = valChar[i];
            }
        }
    }
}
