using MessageStudio.Core.Common;
using MessageStudio.Core.Formats.BinaryText.Structures;
using MessageStudio.Core.Formats.BinaryText.Structures.Sections;
using System.Runtime.CompilerServices;

namespace MessageStudio.Core.Formats.BinaryText;

public struct MsbtReader
{
    public MsbtHeader Header;

    public MsbtAttributeSection? AttributeSection { get; }
    public MsbtLabelSection LabelSection { get; }
    public MsbtTextSection TextSection { get; }

    public MsbtReader(MemoryReader reader)
    {
        if ((reader.Endianness = (Header = new MsbtHeader(reader)).ByteOrderMark) is Endian.Little) {
            reader.Seek(0);
            Header = new MsbtHeader(reader);
        }
        
        for (int i = 0; i < Header.SectionCount; i++) {
            Span<byte> magic = reader.ReadSpan(4);
            if (magic.SequenceEqual("LBL1"u8)) {
                LabelSection = new(reader);
            }
            if (magic.SequenceEqual("ATR1"u8)) {
                AttributeSection = new(reader);
            }
            else if (magic.SequenceEqual("TXT2"u8)) {
                TextSection = new(reader, Header.Encoding);
            }
        }
        
        if (LabelSection is null) {
            throw new InvalidDataException("No LabelSection found in MSBT!");
        }
        
        if (TextSection is null) {
            throw new InvalidDataException("No TextSection found in MSBT!");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Msbt GetWriter() => new(this);

    public static explicit operator Msbt(MsbtReader parser)
        => parser.GetWriter();
}
