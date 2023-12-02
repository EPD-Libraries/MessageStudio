using MessageStudio.Core.Common;

namespace MessageStudio.Core.Formats.BinaryText.Structures;

public enum Encoding : byte
{
    UTF8 = 0,
    Unicode = 1,
}

public ref struct MsbtHeader
{
    public ReadOnlySpan<byte> Magic = "MsgStdBn"u8;
    public Endian ByteOrderMark = Endian.Big;
    public Encoding Encoding = Encoding.Unicode;
    public byte Version = 1;
    public ushort SectionCount;
    public uint FileSize;

    public unsafe MsbtHeader(ref Parser parser)
    {
        Magic = parser.ReadSpan(8);
        ByteOrderMark = parser.Read<Endian>();
        parser.Move(2);

        Encoding = parser.Read<Encoding>();
        Version = parser.Read<byte>();
        SectionCount = parser.Read<ushort>();
        parser.Move(2);

        FileSize = parser.Read<uint>();
        parser.Move(0xA);
    }
}
