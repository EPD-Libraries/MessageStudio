using MessageStudio.Core.Common;
using MessageStudio.Core.Formats.BinaryText.Structures.Common;
using System;
using System.Buffers.Binary;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MessageStudio.Core.Formats.BinaryText.Structures.Sections;

public unsafe class MsbtAttributeSection : IEnumerable<MsbtAttribute>
{
    private readonly MemoryReader? _reader;
    private readonly ushort* _strings;
    private readonly int _firstOffset;
    private MsbtAttribute[]? _cache;

    public int AttributeSize { get; }
    public int Count { get; }

    public unsafe MsbtAttribute this[int index] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (_cache ??= CacheEntries())[index];
    }

    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MsbtAttributeSection(MemoryReader reader)
    {
        SectionHeader header = reader.ReadStruct<SectionHeader>();
        int tableOffset = reader.Position;

        Count = reader.Read<int>();
        AttributeSize = reader.Read<int>();

        if (AttributeSize == 0) {
            goto Cleanup;
        }

        if (AttributeSize != 4) {
            throw new NotSupportedException("Only uint32 and nullptr attribute offsets are supported");
        }

        int offsetBufferSize = Count * AttributeSize;

        Memory<byte> offsetBuffer = reader.Read(offsetBufferSize);
        _reader = new(offsetBuffer, reader.Endianness);
        _firstOffset = _reader.Read<int>();

        Span<ushort> stringsBuffer = MemoryMarshal.Cast<byte, ushort>(reader.ReadSpan(header.SectionSize - (reader.Position - tableOffset)));

        if (_reader.IsNotSystemByteOrder()) {
            // TODO: This is a rather inefficient way of reversing the endianness
            for (int i = 0; i < stringsBuffer.Length; i++) {
                stringsBuffer[i] = BinaryPrimitives.ReverseEndianness(stringsBuffer[i]);
            }
        }

        fixed (ushort* ptr = stringsBuffer) {
            _strings = ptr;
        }

    Cleanup:
        reader.Align(0x10);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private MsbtAttribute[] CacheEntries()
    {
        MsbtAttribute[] result = new MsbtAttribute[Count];
        if (_reader is null) {
            return result;
        }

        for (int i = 0; i < Count; i++) {
            int offset = _reader.Read<int>(AttributeSize * i) - _firstOffset;
            result[i] = new(i, _strings + (offset / 2));
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerator<MsbtAttribute> GetEnumerator()
        => new Enumerator(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public struct Enumerator(MsbtAttributeSection section) : IEnumerator<MsbtAttribute>
    {
        private readonly MsbtAttributeSection _section = section;
        private int _index = -1;

        readonly object IEnumerator.Current {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Current;
        }

        public readonly MsbtAttribute Current {
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
