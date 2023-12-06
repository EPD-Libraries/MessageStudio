using MessageStudio.Core.Common;
using System.Text;

namespace MessageStudio.Core.Formats.BinaryText.Structures;

internal class MsbtEndTag
{
    public static void ToText(ref StringBuilder sb, ushort group, ushort type)
    {
        sb.Append("<[");
        sb.Append(group);
        sb.Append('|');
        sb.Append(type);
        sb.Append("]>");
    }

    public static void ToBinary(ref MemoryWriter writer, ReadOnlySpan<char> endTag)
    {
        int typeIndex = endTag.IndexOf('|');
        int endIndex = endTag.IndexOf(']');

        ReadOnlySpan<char> group = endTag[2..typeIndex];
        ReadOnlySpan<char> type = endTag[++typeIndex..endIndex];

        writer.Write<ushort>(0xF);
        writer.Write(ushort.Parse(group));
        writer.Write(ushort.Parse(type));
    }
}
