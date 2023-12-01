using MessageStudio.Core.Common;
using MessageStudio.Core.Formats.Msbt.Structures.Common;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace MessageStudio.Core.Formats.Msbt.Structures.Sections;

public readonly ref struct MsbtLabelSection
{
    private readonly int _sectionOffset;
    private readonly ReadOnlySpan<MsbtGroup> _groups;
    private readonly ReadOnlySpan<byte> _labelBuffer;

    public MsbtLabelSection(ref Parser parser)
    {
        SectionHeader header = parser.Read<SectionHeader>();
        _sectionOffset = parser.Position;

        int groupCount = parser.Read<int>();
        _groups = parser.ReadSpan<MsbtGroup>(groupCount);
        _labelBuffer = parser.ReadSpan(header.Size, _sectionOffset);

        parser.Seek(_sectionOffset + header.Size);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 8)]
    public readonly struct MsbtGroup
    {
        public readonly int Count;
        public readonly int GroupOffset;
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

        public MsbtLabel(ReadOnlySpan<byte> buffer)
        {
            Parser parser = new(buffer);
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
                return new(_section._labelBuffer[offset..(offset + valueLength + sizeof(int))]);
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
