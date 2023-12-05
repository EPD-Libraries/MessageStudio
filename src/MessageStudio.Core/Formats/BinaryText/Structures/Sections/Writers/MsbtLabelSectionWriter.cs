using MessageStudio.Core.Common;
using MessageStudio.Core.Formats.BinaryText.Structures.Common;

namespace MessageStudio.Core.Formats.BinaryText.Structures.Sections.Writers;

internal static class MsbtLabelSectionWriter
{
    private const int HeaderSize = 16;

    public static void Write(ref MemoryWriter writer, ICollection<string> labels)
    {
        writer.Move(HeaderSize);
        long sectionOffset = writer.Position;

        // Always write with constant
        // group count
        writer.Write(1);
        writer.Write(labels.Count);
        writer.Write(sizeof(uint) + sizeof(uint) + sizeof(uint));

        int index = 0;
        foreach (var label in labels) {
            writer.Write((byte)label.Length);
            writer.WriteUtf8String(label);
            writer.Write(index++);
        }

        long sectionSize = writer.Position - sectionOffset;

        writer.Seek(sectionOffset - HeaderSize);
        writer.Write("LBL1"u8);
        writer.Write(new SectionHeader((int)sectionSize));

        writer.Move(sectionSize);
        writer.Align(0x10);
    }
}
