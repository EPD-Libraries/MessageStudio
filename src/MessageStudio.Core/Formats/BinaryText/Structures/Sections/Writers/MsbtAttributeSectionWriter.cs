using MessageStudio.Core.Common;
using MessageStudio.Core.Formats.BinaryText.Structures.Common;

namespace MessageStudio.Core.Formats.BinaryText.Structures.Sections.Writers;

internal static class MsbtAttributeSectionWriter
{
    private const int HeaderSize = 16;

    public static void Write(ref MemoryWriter writer, Encoding encoding, string?[] attributes)
    {
        writer.Move(HeaderSize);
        long sectionOffset = writer.Position;

        writer.Write(attributes.Length);
        writer.Write(sizeof(uint));

        int firstOffset = attributes.Length * sizeof(uint) + sizeof(uint) + sizeof(uint);

        if (encoding is Encoding.UTF8) {
            WriteUtf8(ref writer, attributes, firstOffset);
        }
        else {
            WriteUtf16(ref writer, attributes, firstOffset);
        }

        long sectionSize = writer.Position - sectionOffset;

        writer.Seek(sectionOffset - HeaderSize);
        writer.Write("ATR1"u8);
        writer.Write(new SectionHeader((int)sectionSize));

        writer.Move(sectionSize);
        writer.Align(0x10);
    }

    private static void WriteUtf8(ref MemoryWriter writer, string?[] attributes, int firstOffset)
    {
        int offset = firstOffset;
        foreach (var attribute in attributes) {
            writer.Write(offset);
            offset += sizeof(byte) + (attribute?.Length ?? 0);
        }

        foreach (var attribute in attributes) {
            if (attribute is not null) {
                writer.WriteUtf8String(attribute);
            }

            writer.Write<byte>(0);
        }
    }

    private static void WriteUtf16(ref MemoryWriter writer, string?[] attributes, int firstOffset)
    {
        int offset = firstOffset;
        foreach (var attribute in attributes) {
            writer.Write(offset);
            offset += sizeof(ushort) + (attribute?.Length * 2 ?? 0);
        }

        foreach (var attribute in attributes) {
            if (attribute is not null) {
                writer.WriteUtf16String(attribute);
            }

            writer.Write<ushort>(0);
        }
    }
}
