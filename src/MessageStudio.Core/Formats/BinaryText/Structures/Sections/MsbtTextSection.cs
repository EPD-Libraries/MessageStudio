using MessageStudio.Core.Common;
using MessageStudio.Core.Formats.BinaryText.Structures.Common;
using System.Collections;
using System.Runtime.CompilerServices;

namespace MessageStudio.Core.Formats.BinaryText.Structures.Sections;

public unsafe class MsbtTextSection : IEnumerable<MsbtText>
{
    private const int OffsetSize = sizeof(uint);

    private readonly MemoryReader _reader;
    private readonly Encoding _encoding;
    private readonly byte* _strings;
    private readonly int _firstOffset;
    private readonly int _endOfSection;
    private MsbtText[]? _cache;

    public int Count { get; }

    public unsafe MsbtText this[int index] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (_cache ??= CacheEntries())[index];
    }

    public MsbtTextSection(MemoryReader reader, Encoding encoding)
    {
        _encoding = encoding;

        SectionHeader header = reader.ReadStruct<SectionHeader>();
        int tableOffset = reader.Position;

        Count = reader.Read<int>();
        int offsetBufferSize = Count * OffsetSize;

        Memory<byte> offsetBuffer = reader.Read(offsetBufferSize);
        _reader = new(offsetBuffer, reader.Endianness);
        _firstOffset = _reader.Read<int>();
        _endOfSection = header.SectionSize - _firstOffset;

        fixed (byte* ptr = reader.ReadSpan(header.SectionSize - (reader.Position - tableOffset))) {
            _strings = ptr;
        }

        reader.Align(0x10);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private MsbtText[] CacheEntries()
    {
        MsbtText[] result = new MsbtText[Count];
        for (int i = 0; i < Count; i++) {
            int offset = _reader.Read<int>(OffsetSize * i) - _firstOffset;
            int length = (_reader.Position >= _reader.Length ? _endOfSection : _reader.Read<int>(OffsetSize * (i + 1)) - _firstOffset) - offset;
            result[i] = new(i, _strings + offset, length, _encoding, _reader.Endianness);
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerator<MsbtText> GetEnumerator()
        => new Enumerator(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public struct Enumerator(MsbtTextSection section) : IEnumerator<MsbtText>
    {
        private readonly MsbtTextSection _section = section;
        private int _index = -1;

        readonly object IEnumerator.Current {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Current;
        }

        public readonly MsbtText Current {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _section[_index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() => ++_index < _section.Count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            _index = -1;
        }

        public readonly void Dispose() { }
    }
}
