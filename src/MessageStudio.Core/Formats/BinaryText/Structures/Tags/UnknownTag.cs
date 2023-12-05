using MessageStudio.Core.Common;
using MessageStudio.Core.Common.Extensions;
using System.Runtime.InteropServices;
using System.Text;

namespace MessageStudio.Core.Formats.BinaryText.Structures.Tags;

public unsafe class UnknownTag(ushort* data, int dataSize, ushort group, ushort type) : IMsbtTag
{
    private readonly ushort* _data = data;
    private readonly int _dataSize = dataSize;
    private readonly ushort _group = group;
    private readonly ushort _type = type;

    public static IMsbtTag FromBinary(in ushort group, in ushort type, in Span<ushort> data)
    {
        fixed (ushort* ptr = data) {
            return new UnknownTag(ptr, data.Length, group, type);
        }
    }

    public static IMsbtTag FromText(ReadOnlySpan<char> text)
    {
        ushort group = ushort.Parse(text[1..text.IndexOf(' ')]);
        ushort type = ushort.Parse(text.ReadProperty("Type"));
        ReadOnlySpan<byte> data = Convert.FromHexString(text.ReadProperty("Data")[2..]);

        fixed (ushort* ptr = MemoryMarshal.Cast<byte, ushort>(data)) {
            return new UnknownTag(ptr, data.Length / 2, group, type);
        }
    }

    public void ToBinary(ref MemoryWriter writer)
    {
        writer.Write<ushort>(0xE);
        writer.Write(_group);
        writer.Write(_type);
        writer.Write((ushort)(_dataSize * 2));

        if (writer.IsNotSystemByteOrder()) {
            for (int i = 0; i < _dataSize; i++) {
                writer.Write(_data[i]);
            }
        }
        else {
            ReadOnlySpan<ushort> buffer = new(_data, _dataSize);
            writer.Write(MemoryMarshal.Cast<ushort, byte>(buffer));
        }
    }

    public void ToText(ref StringBuilder sb)
    {
        sb.Append('<');
        sb.Append(_group);
        sb.Append(" Type='");
        sb.Append(_type);
        sb.Append("' Data='0x");

        ReadOnlySpan<ushort> span = new(_data, _dataSize);
        sb.Append(Convert.ToHexString(MemoryMarshal.Cast<ushort, byte>(span)));
        sb.Append("' />");
    }
}
