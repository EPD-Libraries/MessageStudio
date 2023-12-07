namespace MessageStudio.IO;

public enum TextEncoding : byte
{
    UTF8 = 0,
    Unicode = 1,
}

public enum Endian : ushort
{
    Big = 0xFEFF,
    Little = 0xFFFE,
}