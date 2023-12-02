using MessageStudio.Core.Common;
using MessageStudio.Core.Formats.BinaryText.Structures;
using MessageStudio.Core.Formats.BinaryText.Structures.Sections;
using System.Runtime.CompilerServices;

namespace MessageStudio.Core.Formats.BinaryText;

public ref struct MsbtReader
{
    public MsbtHeader Header;

    public MsbtAttributeSection AttributeSection;
    public MsbtLabelSection LabelSection;
    public MsbtTextSection TextSection;

    public MsbtReader(ref Parser parser)
    {
        if ((parser.Endian = (Header = new MsbtHeader(ref parser)).ByteOrderMark) is Endian.Little) {
            parser.Seek(0);
            Header = new MsbtHeader(ref parser);
        }

        for (int i = 0; i < Header.SectionCount; i++) {
            Span<byte> magic = parser.ReadSpan(4);
            if (magic.SequenceEqual("LBL1"u8)) {
                LabelSection = new(ref parser);
            }
            else if (magic.SequenceEqual("ATR1"u8)) {
                AttributeSection = new(ref parser);
            }
            else if (magic.SequenceEqual("TXT2"u8)) {
                TextSection = new(ref parser);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Msbt GetWriter() => new(this);

    public static explicit operator Msbt(MsbtReader parser)
        => parser.GetWriter();
}
