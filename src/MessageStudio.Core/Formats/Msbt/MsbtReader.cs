using MessageStudio.Core.Common;
using MessageStudio.Core.Formats.Msbt.Structures;
using MessageStudio.Core.Formats.Msbt.Structures.Sections;
using System.Runtime.CompilerServices;

namespace MessageStudio.Core.Formats.Msbt;

public ref struct MsbtReader
{
    public MsbtHeader Header;

    public MsbtAttributeSection AttributeSection;
    public MsbtLabelSection LabelSection;
    public MsbtTextSection TextSection;

    public MsbtReader(ref Parser parser)
    {
        Header = new MsbtHeader(ref parser);
        parser.Endian = Header.ByteOrderMark;

        for (int i = 0; i < Header.SectionCount; i++) {
            if (parser.CheckForMagic("LBL1"u8)) {
                LabelSection = new(ref parser);
            }
            else if (parser.CheckForMagic("ATR1"u8)) {
                AttributeSection = new(ref parser);
            }
            else if (parser.CheckForMagic("TXT2"u8)) {
                TextSection = new(ref parser);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly MsbtWriter GetWriter() => new(this);

    public static explicit operator MsbtWriter(MsbtReader parser)
        => parser.GetWriter();
}
