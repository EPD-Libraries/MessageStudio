﻿using MessageStudio.Core.Formats.BinaryText.Readers;
using MessageStudio.Core.Formats.BinaryText.Structures;
using MessageStudio.Core.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace MessageStudio.Core.Formats.BinaryText;

public readonly ref struct ImmutableMsbt
{
    public readonly AttributeSectionReader AttributeSectionReader;
    public readonly LabelSectionReader LabelSectionReader;
    public readonly TextSectionReader TextSectionReader;

    public ImmutableMsbt(ref SpanReader reader)
    {
        ref MsbtHeader header = ref reader.Read<MsbtHeader, MsbtHeader.Reverser>();
        if (header.ByteOrderMark is Endian.Little) {
            // Reverse the buffer back to LE
            // since it's initially read in BE
            MsbtHeader.Reverser.Reverse(reader.Read(Unsafe.SizeOf<MsbtHeader>(), 0));
            reader.Endianness = Endian.Little;
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
    }

    public readonly void WriteYaml(in StringBuilder sb)
    {

    }
}