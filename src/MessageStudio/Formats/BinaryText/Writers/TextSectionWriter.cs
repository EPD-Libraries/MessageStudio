using MessageStudio.Common;
using MessageStudio.Formats.BinaryText.Extensions;
using Revrs;
using System.Runtime.CompilerServices;

namespace MessageStudio.Formats.BinaryText.Writers;

internal static class TextSectionWriter
{
    public static void Write(ref RevrsWriter writer, TextEncoding encoding, string?[] entries)
    {
        long sectionOffset = writer.Position;

        writer.Write(entries.Length);

        int firstOffset = entries.Length * sizeof(uint) + sizeof(uint);
        long sectionEndPosition;

        if (encoding == TextEncoding.UTF8) {
            sectionEndPosition = WriteUtf8(ref writer, entries, firstOffset, sectionOffset);
        }
        else {
            sectionEndPosition = WriteUtf16(ref writer, entries, firstOffset, sectionOffset);
        }

        writer.Seek(sectionEndPosition);
    }

    private static long WriteUtf8(ref RevrsWriter writer, string?[] entries, int firstOffset, long sectionOffset)
    {
        throw new NotSupportedException("UTF8 encoded MSBT files are not supported");
    }

    private static long WriteUtf16(ref RevrsWriter writer, string?[] entries, int firstOffset, long sectionOffset)
    {
        long offsetsPosition = writer.Position;
        writer.Move(firstOffset - sizeof(uint));

        Span<long> offsets = entries.Length * sizeof(long) < 0xF0000
            ? stackalloc long[entries.Length] : new long[entries.Length];

        for (int i = 0; i < entries.Length; i++) {
            WriteUtf16Entry(ref writer, entries[i]);
            writer.Write<ushort>(0);

            offsets[i] = writer.Position - firstOffset - sectionOffset;
        }

        long sectionEndPosition = writer.Position;
        int relativeOffsetCount = entries.Length - 1;

        writer.Seek(offsetsPosition);
        writer.Write(firstOffset);
        for (int i = 0; i < relativeOffsetCount; i++) {
            writer.Write((uint)(offsets[i] + firstOffset));
        }

        return sectionEndPosition;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteUtf16Entry(ref RevrsWriter writer, ReadOnlySpan<char> text)
    {
        for (int i = 0; i < text.Length; i++) {
            char value = text[i];
            int endTagIndex;
            if (value == '<' && (endTagIndex = text[i..].IndexOf('>')) > -1) {
                ReadOnlySpan<char> tagStr = text[i..((i += endTagIndex) + 1)];
                if (tagStr.Length > 1 && tagStr[1] == '[') {
                    writer.WriteEndTag(tagStr, TextEncoding.Unicode);
                }
                else {
                    writer.WriteTag(tagStr, TextEncoding.Unicode);
                }
            }
            else {
                writer.Write(value);
            }
        }
    }
}
