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
        throw new NotImplementedException();
    }

    public byte[] ToBinary()
    {
        throw new NotImplementedException();
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
