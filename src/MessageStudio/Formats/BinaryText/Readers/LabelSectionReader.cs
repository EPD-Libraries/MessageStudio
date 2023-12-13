using MessageStudio.Formats.BinaryText.Structures;
using Revrs;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MessageStudio.Formats.BinaryText.Readers;

public readonly ref struct LabelSectionReader
{
    public readonly Span<byte> Data;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LabelSectionReader(ref RevrsReader reader, ref MsbtSectionHeader header)
    {
        if (reader.Endianness.IsNotSystemEndianness()) {
            int sectionOffset = reader.Position;
            int groupCount = reader.Read<int>();
            reader.ReverseSpan<MsbtLabelGroup, MsbtLabelGroup.Reverser>(groupCount);

            int eos = sectionOffset + header.SectionSize;
            while (reader.Position < eos) {
                byte size = reader.Read<byte>();
                reader.Move(size);
                reader.Reverse<uint>();
            }

            reader.Seek(sectionOffset);
        }

        // Store the section buffer for later
        Data = reader.Read(header.SectionSize);
    }

    public readonly MsbtLabel this[ReadOnlySpan<byte> key] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            int index = Data.IndexOf(key);
            byte size = Data[index - 1];
            return new(size, Data[index..(index + size + sizeof(uint))]);
        }
    }

    public Enumerator GetEnumerator()
        => new(this);

    public ref struct Enumerator
    {
        private readonly Span<byte> _buffer;
        private readonly Span<MsbtLabelGroup> _groups;
        private readonly int _groupCount;
        private int _groupIndex;
        private int _labelIndex;
        private int _position;

        public Enumerator(LabelSectionReader labelSectionReader)
        {
            RevrsReader reader = RevrsReader.Native(_buffer = labelSectionReader.Data);
            _groupCount = reader.Read<int>();
            _groups = reader.ReadSpan<MsbtLabelGroup, MsbtLabelGroup.Reverser>(_groupCount);
            _position = reader.Position;
        }

        public MsbtLabel Current {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                byte size = _buffer[_position];
                return new(size, _buffer[++_position..(_position += size + sizeof(uint))]);
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
