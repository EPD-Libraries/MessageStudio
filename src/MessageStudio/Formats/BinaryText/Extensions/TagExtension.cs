using MessageStudio.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace MessageStudio.Formats.BinaryText.Extensions;

public static class TagExtension
{
    // TODO: Tag reading/writing
    // 
    // Sometime in the future this should support loading
    // a schema and writing tags according to the schema.

    internal static void WriteTag(this StringBuilder sb, ushort group, ushort tag, Span<byte> data)
    {
        sb.Append('<');
        sb.Append(group);
        sb.Append(" Type='");
        sb.Append(tag);

        if (!data.IsEmpty) {
            sb.Append("' Data='0x");
            sb.Append(Convert.ToHexString(data));
        }

        sb.Append("'/>");
    }

    internal static void WriteTag(this InternalWriter writer, ReadOnlySpan<char> text, Encoding encoding)
    {
        ReadOnlySpan<char> group = text.ReadTagName();
        ReadOnlySpan<char> type = text.ReadProperty("Type");
        ReadOnlySpan<char> hexData = text.ReadProperty("Data");
        ReadOnlySpan<byte> data = hexData.IsEmpty
            ? [] : Convert.FromHexString(hexData[2..]);

        if (encoding == Encoding.UTF8) {
            writer.Write<byte>(0xE);
            writer.Write(byte.Parse(group));
            writer.Write(byte.Parse(type));
            writer.Write((byte)data.Length);
            writer.Write(data);
        }
        else {
            writer.Write<ushort>(0xE);
            writer.Write(ushort.Parse(group));
            writer.Write(ushort.Parse(type));
            writer.Write((ushort)data.Length);

            if (writer.IsNotSystemByteOrder()) {
                ReadOnlySpan<ushort> utf16 = MemoryMarshal.Cast<byte, ushort>(data);
                for (int i = 0; i < utf16.Length; i++) {
                    writer.Write(utf16[i]);
                }
            }
            else {
                writer.Write(data);
            }
        }
    }

    internal static void WriteEndTag(this StringBuilder sb, ushort group, ushort tag)
    {
        sb.Append("<[");
        sb.Append(group);
        sb.Append('|');
        sb.Append(tag);
        sb.Append("]>");
    }

    internal static void WriteEndTag(this InternalWriter writer, ReadOnlySpan<char> text, Encoding encoding)
    {
        int typeIndex = text.IndexOf('|');
        int endIndex = text.IndexOf(']');

        ReadOnlySpan<char> group = text[2..typeIndex];
        ReadOnlySpan<char> type = text[++typeIndex..endIndex];

        if (encoding == Encoding.UTF8) {
            writer.Write<byte>(0xF);
            writer.Write(byte.Parse(group));
            writer.Write(byte.Parse(type));
        }
        else {
            writer.Write<ushort>(0xF);
            writer.Write(ushort.Parse(group));
            writer.Write(ushort.Parse(type));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ReadOnlySpan<char> ReadTagName(in this ReadOnlySpan<char> text)
    {
        return text[1..text.IndexOf(' ')];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ReadOnlySpan<char> ReadProperty(in this ReadOnlySpan<char> text, in ReadOnlySpan<char> name)
    {
        int startIndex = text.IndexOf(name);
        if (startIndex <= 0) {
            return [];
        }

        startIndex += name.Length + 2;
        int endIndex = startIndex + text[startIndex..].IndexOf('\'');
        return text[startIndex..endIndex];
    }
}
