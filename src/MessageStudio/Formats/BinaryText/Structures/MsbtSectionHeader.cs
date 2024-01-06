using Revrs;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MessageStudio.Formats.BinaryText.Structures;

[StructLayout(LayoutKind.Explicit, Pack = 4, Size = 16)]
public readonly struct MsbtSectionHeader
{
    [FieldOffset(0x0)]
    public readonly uint Magic;

    [FieldOffset(0x4)]
    public readonly int SectionSize;

    public MsbtSectionHeader() { }
    public MsbtSectionHeader(uint magic, int sectionSize)
    {
        Magic = magic;
        SectionSize = sectionSize;
    }

    internal static void WriteSection(in RevrsWriter writer, ref ushort sectionCount, uint magic, Action writeSection)
    {
        sectionCount++;
        int headerSize = Unsafe.SizeOf<MsbtSectionHeader>();
        writer.Move(headerSize);
        long sectionOffset = writer.Position;

        writeSection();
        long sectionSize = writer.Position - sectionOffset;

        MsbtSectionHeader header = new(magic, (int)sectionSize);
        writer.Seek(sectionOffset - headerSize);
        writer.Write<MsbtSectionHeader, Reverser>(header);
        writer.Move(sectionSize);
        writer.Align(0x10);
    }

    public class Reverser : IStructReverser
    {
        public static void Reverse(in Span<byte> buffer)
        {
            buffer[0x4..0x8].Reverse();
        }
    }
}
