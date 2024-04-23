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

        long offsetsPosition = writer.Position;
        writer.Move(firstOffset - sizeof(uint));

        Span<long> offsets = entries.Length * sizeof(long) < 0xF0000
            ? stackalloc long[entries.Length] : new long[entries.Length];

        for (int i = 0; i < entries.Length; i++) {
            WriteEntry(ref writer, entries[i], encoding);
            offsets[i] = writer.Position - firstOffset - sectionOffset;
        }

        long sectionEndPosition = writer.Position;
        int relativeOffsetCount = entries.Length - 1;

        writer.Seek(offsetsPosition);
        writer.Write(firstOffset);
        for (int i = 0; i < relativeOffsetCount; i++) {
            writer.Write((uint)(offsets[i] + firstOffset));
        }

        writer.Seek(sectionEndPosition);
    }

    private static void WriteEntry(ref RevrsWriter writer, ReadOnlySpan<char> text, TextEncoding encoding)
    {
        for (int i = 0; i < text.Length; i++) {
            char value = text[i];
            int endTagIndex;
            if (value == '<' && (endTagIndex = text[i..].IndexOf('>')) > -1) {
                ReadOnlySpan<char> functionText = text[i..((i += endTagIndex) + 1)];
                writer.WriteFunction(functionText, encoding);
            }
            else {
                writer.Write(value);
            }
        }

        switch (encoding) {
            case TextEncoding.UTF8:
                writer.Write<byte>(0);
                break;
            case TextEncoding.Unicode:
                writer.Write<ushort>(0);
                break;
        }
    }
}
