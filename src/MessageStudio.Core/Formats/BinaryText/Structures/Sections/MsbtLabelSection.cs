using MessageStudio.Core.Common;
using MessageStudio.Core.Formats.BinaryText.Structures.Common;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MessageStudio.Core.Formats.BinaryText.Structures.Sections;

public unsafe class MsbtLabelSection : IEnumerable<MsbtLabel>
{
    private readonly Endian _endianness;
    private readonly MsbtGroup* _groups;
    private readonly int _groupCount;
    private readonly Memory<byte> _labelBuffer;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MsbtLabelSection(MemoryReader reader)
    {
        _endianness = reader.Endianness;
        
        SectionHeader header = reader.ReadStruct<SectionHeader>();
        int sectionOffset = reader.Position;
        
        int groupCount = reader.Read<int>();
        Span<MsbtGroup> groups = reader.ReadSpan<MsbtGroup>(groupCount);

        // TODO: Unnecessary allocation?
        _labelBuffer = reader.Read(header.SectionSize, sectionOffset);
        
        _groupCount = groups.Length;
        fixed (MsbtGroup* ptr = groups) {
            _groups = ptr;
        }
        
        reader.Align(0x10);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerator<MsbtLabel> GetEnumerator()
        => new Enumerator(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public struct Enumerator(MsbtLabelSection section) : IEnumerator<MsbtLabel>
    {
        private readonly MsbtLabelSection _section = section;
        private int _groupIndex = 0;
        private int _labelIndex = 0;
        private int _labelOffset = 0;

        object IEnumerator.Current {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Current;
        }

        public MsbtLabel Current {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                int offset = _section._groups[_groupIndex].GroupOffset + _labelOffset;
                byte valueLength = _section._labelBuffer.Span[offset++];
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

            if (++_groupIndex >= _section._groupCount) {
                return false;
            }

            _labelIndex = 0;
            _labelOffset = 0;
            goto MoveNext;
        }

        public void Reset()
        {
            _labelIndex = _groupIndex = _labelOffset = 0;
        }

        public readonly void Dispose() { }
    }
}
