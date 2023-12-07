using MessageStudio.IO;

namespace MessageStudio.Formats.BinaryText.Writers;

internal static class LabelSectionWriter
{
    public static void Write(InternalWriter writer, ICollection<string> labels)
    {
        writer.Write(1);
        writer.Write(labels.Count);
        writer.Write(sizeof(uint) + sizeof(uint) + sizeof(uint));

        int index = 0;
        foreach (var label in labels) {
            writer.Write((byte)label.Length);
            writer.WriteUtf8String(label);
            writer.Write(index++);
        }
    }
}
