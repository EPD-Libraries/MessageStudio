using MessageStudio.IO;

namespace MessageStudio.Formats.BinaryText.Writers;

internal static class AttributeSectionWriter
{
    public static void Write(in InternalWriter writer, TextEncoding encoding, string?[] attributes)
    {
        writer.Write(attributes.Length);
        writer.Write(sizeof(uint));

        int firstOffset = attributes.Length * sizeof(uint) + sizeof(uint) + sizeof(uint);

        if (encoding is TextEncoding.UTF8) {
            WriteUtf8(writer, attributes, firstOffset);
        }
        else {
            WriteUtf16(writer, attributes, firstOffset);
        }
    }

    private static void WriteUtf8(in InternalWriter writer, string?[] attributes, int firstOffset)
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

    private static void WriteUtf16(InternalWriter writer, string?[] attributes, int firstOffset)
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
