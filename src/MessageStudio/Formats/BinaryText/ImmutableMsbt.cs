using MessageStudio.Formats.BinaryText.Readers;
using MessageStudio.Formats.BinaryText.Structures;
using Revrs;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;

namespace MessageStudio.Formats.BinaryText;

public readonly ref struct ImmutableMsbt
{
    public readonly MsbtHeader Header;
    public readonly AttributeSectionReader AttributeSectionReader;
    public readonly LabelSectionReader LabelSectionReader;
    public readonly TextSectionReader TextSectionReader;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ImmutableMsbt(ref RevrsReader reader)
    {
        ref MsbtHeader header = ref reader.Read<MsbtHeader, MsbtHeader.Reverser>();
        if (header.ByteOrderMark is Endianness.Little) {
            // Reverse the buffer back to LE
            // since it's initially read in BE
            reader.Reverse<MsbtHeader, MsbtHeader.Reverser>(0);
            reader.Endianness = Endianness.Little;
        }

        if (header.Magic != Msbt.MSBT_MAGIC) {
            throw new InvalidDataException("Invalid MSBT magic!");
        }

        for (int i = 0; i < header.SectionCount; i++) {
            ref MsbtSectionHeader sectionHeader = ref reader.Read<MsbtSectionHeader, MsbtSectionHeader.Reverser>();
            if (sectionHeader.Magic == Msbt.ATR1_MAGIC) {
                AttributeSectionReader = new(ref reader, ref sectionHeader, header.Encoding);
            }
            else if (sectionHeader.Magic == Msbt.LBL1_MAGIC) {
                LabelSectionReader = new(ref reader, ref sectionHeader);
            }
            else if (sectionHeader.Magic == Msbt.TXT2_MAGIC) {
                TextSectionReader = new(ref reader, ref sectionHeader, header.Encoding);
            }
            else {
                // TODO: convert the Magic to
                // a string before throwing
                throw new NotSupportedException(
                    $"Unsupported MSBT section {sectionHeader.Magic}");
            }

            reader.Align(0x10);
        }

        Header = header;
    }
}
