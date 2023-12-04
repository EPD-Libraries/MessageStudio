using MessageStudio.Core.Common;

namespace MessageStudio.Core.Formats.BinaryText.Structures;

public enum Encoding : byte
{
    UTF8 = 0,
    Unicode = 1,
}

public struct MsbtHeader
{
    public const string Magic = "MsgStdBn";

    public Endian ByteOrderMark = Endian.Big;
    public Encoding Encoding = Encoding.Unicode;
    public byte Version = 1;
    public ushort SectionCount;
    public uint FileSize;

    public MsbtHeader(MemoryReader reader)
    {
        Span<byte> magic = reader.ReadSpan(8);
        if (!magic.SequenceEqual("MsgStdBn"u8)) {
            throw new InvalidDataException("Invalid MSBT magic");
        }

        ByteOrderMark = reader.Read<Endian>();
        reader.Move(2);

        Encoding = reader.Read<Encoding>();
        Version = reader.Read<byte>();
        SectionCount = reader.Read<ushort>();
        reader.Move(2);

        FileSize = reader.Read<uint>();
        reader.Move(0xA);
    }
}
