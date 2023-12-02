using MessageStudio.Core.Common;
using MessageStudio.Core.Formats.BinaryText.Structures.Common;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace MessageStudio.Core.Formats.BinaryText.Structures.Sections;

public readonly ref struct MsbtLabelSection
{
    private readonly Endian _endianness;
    private readonly Span<MsbtGroup> _groups;
    private readonly Span<byte> _labelBuffer;

    public MsbtLabelSection(ref Parser parser)
    {
        _endianness = parser.Endian;

        SectionHeader header = parser.ReadStruct<SectionHeader>();
        int sectionOffset = parser.Position;

        int groupCount = parser.Read<int>();
        _groups = parser.ReadSpan<MsbtGroup>(groupCount);
        _labelBuffer = parser.ReadSpan(header.Size, sectionOffset);

        parser.Align(0x10);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 8)]
    private readonly struct MsbtGroup : IReversable
    {
        public readonly int Count;
        public readonly int GroupOffset;

        public static void Reverse(in Span<byte> buffer)
        {
            buffer[0..4].Reverse();
            buffer[4..8].Reverse();
        }
    }

    public readonly unsafe struct MsbtLabel
    {
        private readonly int _valueLength;
        private readonly byte* _valuePtr;

        public readonly int Index;

        public string Value {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Utf8StringMarshaller.ConvertToManaged(_valuePtr)![.._valueLength];
        }

        public MsbtLabel(Span<byte> buffer, Endian endian)
        {
            Parser parser = new(buffer, endian);
            _valueLength = buffer.Length - 4;
            fixed (byte* ptr = buffer[.._valueLength]) {
                _valuePtr = ptr;
            }

            parser.Move(_valueLength);
            Index = parser.Read<int>();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator() => new(this);

    public ref struct Enumerator(MsbtLabelSection section)
    {
        private readonly MsbtLabelSection _section = section;
        private int _groupIndex = 0;
        private int _labelIndex = 0;
        private int _labelOffset = 0;

        public MsbtLabel Current {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                int offset = _section._groups[_groupIndex].GroupOffset + _labelOffset;
                byte valueLength = _section._labelBuffer[offset++];
                _labelOffset += valueLength + sizeof(int) + sizeof(byte);
                return new(_section._labelBuffer[offset..(offset + valueLength + sizeof(int))], _section._endianness);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
        MoveNext:
            if (_section._groups[_groupIndex].Count > _labelIndex) {
                _labelIndex++;
                return true;
            }

            if (++_groupIndex >= _section._groups.Length) {
                return false;
            }

            _labelIndex = 0;
            _labelOffset = 0;
            goto MoveNext;
        }
    }
}
