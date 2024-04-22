using MessageStudio.Formats.BinaryText.Readers;
using MessageStudio.Formats.BinaryText.Structures;
using Revrs;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;

namespace MessageStudio.Formats.BinaryText;

public readonly ref struct ImmutableMsbt
{
    public readonly MsbtHeader Header;
    public readonly AttributeSectionReader AttributeSectionReader;
    public readonly LabelSectionReader LabelSectionReader;
    public readonly TextSectionReader TextSectionReader;

    public unsafe ImmutableMsbtEntry this[string label] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            byte* ptr = Utf8StringMarshaller.ConvertToUnmanaged(label);
            ReadOnlySpan<byte> utf8LabelBytes = new(ptr, label.Length);
            return this[utf8LabelBytes];
        }
    }

    public ImmutableMsbtEntry this[ReadOnlySpan<byte> label] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            MsbtLabel msbtLabel = LabelSectionReader[label];
            return new(
                msbtLabel,
                AttributeSectionReader[msbtLabel.Index],
                TextSectionReader[msbtLabel.Index]
            );
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ImmutableMsbt(ref RevrsReader reader)
    {
        ref MsbtHeader header = ref reader.Read<MsbtHeader, MsbtHeader.Reverser>();
        if (header.ByteOrderMark != reader.Endianness) {
            // Reverse the buffer back to LE
            // since it's initially read in BE
            reader.Endianness = header.ByteOrderMark;
            reader.Reverse<MsbtHeader, MsbtHeader.Reverser>(0);
        }

        if (header.Magic != Msbt.MAGIC) {
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
                throw new NotSupportedException($"""
                    Unsupported MSBT section '{Encoding.UTF8.GetString(BitConverter.GetBytes(sectionHeader.Magic))}'
                    """);
            }

            reader.Align(0x10);
        }

        Header = header;
        Header.ByteOrderMark = reader.Endianness;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator()
        => new(this);

    public ref struct Enumerator
    {
        private readonly ImmutableMsbt _msbt;
        private readonly Span<byte> _buffer;
        private readonly Span<MsbtLabelGroup> _groups;
        private readonly int _groupCount;
        private int _groupIndex;
        private int _labelIndex;
        private int _position;

        public Enumerator(ImmutableMsbt msbt)
        {
            _msbt = msbt;
            RevrsReader reader = RevrsReader.Native(_buffer = msbt.LabelSectionReader.Data);
            _groupCount = reader.Read<int>();
            _groups = reader.ReadSpan<MsbtLabelGroup, MsbtLabelGroup.Reverser>(_groupCount);
            _position = reader.Position;
        }

        public ImmutableMsbtEntry Current {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                byte size = _buffer[_position];
                MsbtLabel label = new(size, _buffer[++_position..(_position += size + sizeof(uint))]);

                return new(
                    label,
                    _msbt.AttributeSectionReader[label.Index],
                    _msbt.TextSectionReader[label.Index]
                );
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
        MoveNext:
            if (_groups[_groupIndex].LabelCount > _labelIndex) {
                _labelIndex++;
                return true;
            }

            if (++_groupIndex >= _groupCount) {
                return false;
            }

            _labelIndex = 0;
            goto MoveNext;
        }
    }
}
