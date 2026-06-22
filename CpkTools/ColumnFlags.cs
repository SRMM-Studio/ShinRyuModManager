namespace CpkTools;

public enum StorageFlags
{
    StorageMask = 0xF0,
    StorageNone = 0x00,
    StorageZero = 0x10,
    StorageConstant = 0x30,
    StoragePerrow = 0x50,
}

public enum TypeFlags
{
    TypeMask = 0x0F,
    TypeData = 0x0B,
    TypeString = 0x0A,
    TypeFloat = 0x08,
    Type8Byte2 = 0x07,
    Type8Byte = 0x06,
    Type4Byte2 = 0x05,
    Type4Byte = 0x04,
    Type2Byte2 = 0x03,
    Type2Byte = 0x02,
    Type1Byte2 = 0x01,
    Type1Byte = 0x00,
}
