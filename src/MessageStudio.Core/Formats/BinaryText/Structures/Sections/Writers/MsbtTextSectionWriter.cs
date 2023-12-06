using MessageStudio.Core.Common;
using MessageStudio.Core.Formats.BinaryText.Structures.Common;
using System.Runtime.CompilerServices;

namespace MessageStudio.Core.Formats.BinaryText.Structures.Sections.Writers;

internal static class MsbtTextSectionWriter
{
    private const int HeaderSize = 16;

    public static void Write(ref MemoryWriter writer, Encoding encoding, string[] entries)
    {
        writer.Move(HeaderSize);
        long sectionOffset = writer.Position;

        writer.Write(entries.Length);

        int firstOffset = entries.Length * sizeof(uint) + sizeof(uint);
        long sectionSize = encoding is Encoding.UTF8
            ? WriteUtf8(ref writer, entries, firstOffset, sectionOffset)
            : WriteUtf16(ref writer, entries, firstOffset, sectionOffset);

        writer.Seek(sectionOffset - HeaderSize);
        writer.Write("TXT2"u8);
        writer.WriteStruct(new SectionHeader((int)sectionSize));

        writer.Move(sectionSize);
        writer.Align(0x10);
    }

    private static long WriteUtf8(ref MemoryWriter writer, string[] entries, int firstOffset, long sectionOffset)
    {
        throw new NotSupportedException("UTF8 encoded MSBT files are not supported");
    }

    private static long WriteUtf16(ref MemoryWriter writer, string[] entries, int firstOffset, long sectionOffset)
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

        long endOfSection = writer.Position;
        int relOffsetCount = entries.Length - 1;

        writer.Seek(offsetsPosition);
        writer.Write(firstOffset);
        for (int i = 0; i < relOffsetCount; i++) {
            writer.Write((uint)(offsets[i] + firstOffset));
        }

        return endOfSection - sectionOffset;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteUtf16Entry(ref MemoryWriter writer, ReadOnlySpan<char> text)
    {
        for (int i = 0; i < text.Length; i++) {
            char value = text[i];
            int endTagIndex;
            if (value == '<' && (endTagIndex = text[i..].IndexOf('>')) > -1) {
                IMsbtTag tag = MsbtTagManager.FromText(text[i..((i += endTagIndex) + 1)]);
                tag.ToBinary(ref writer);
            }
            else {
                writer.Write(value);
            }
        }
    }
}
