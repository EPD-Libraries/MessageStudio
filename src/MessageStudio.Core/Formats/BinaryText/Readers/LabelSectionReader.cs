﻿using MessageStudio.Core.Formats.BinaryText.Structures;
using MessageStudio.Core.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace MessageStudio.Core.Formats.BinaryText.Readers;

public readonly ref struct LabelSectionReader
{
    private readonly Span<byte> _buffer;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LabelSectionReader(ref SpanReader reader, ref MsbtSectionHeader header)
    {
        if (reader.IsNotSystemByteOrder()) {
            int sectionOffset = reader.Position;
            int groupCount = reader.Read<int>();
            reader.ReverseSpan<LabelGroup, LabelGroup.Reverser>(groupCount);

            int eos = sectionOffset + header.SectionSize;
            while (reader.Position < eos) {
                byte size = reader.Read<byte>();
                reader.Move(size);
                reader.Reverse<uint>();
            }

            reader.Seek(sectionOffset);
        }

        // Store the section buffer for later
        _buffer = reader.Read(header.SectionSize);
    }

    public readonly LabelMarshal this[ReadOnlySpan<byte> key] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            int index = _buffer.IndexOf(key);
            byte size = _buffer[index - 1];
            return new(size, _buffer[index..(index + size + sizeof(uint))]);
        }
    }

    [StructLayout(LayoutKind.Explicit, Pack = 4, Size = 8)]
    private struct LabelGroup
    {
        /// <summary>
        /// The number of labels in the group
        /// </summary>
        [FieldOffset(0x0)]
        public int LabelCount;

        /// <summary>
        /// Offset to the first label in the section
        /// relative to the beginning of the section
        /// </summary>
        [FieldOffset(0x4)]
        public int LabelOffset;

        public class Reverser : ISpanReverser
        {
            public static void Reverse(in Span<byte> buffer)
            {
                buffer[0x0..0x4].Reverse();
                buffer[0x4..0x8].Reverse();
            }
        }
    }

    public readonly ref struct LabelMarshal(byte size, Span<byte> buffer)
    {
        private readonly byte _size = size;
        private readonly Span<byte> _buffer = buffer;

        public ReadOnlySpan<byte> Label {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer[.._size];
        }

        public ref int Index {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref MemoryMarshal.Cast<byte, int>(_buffer[_size.._buffer.Length])[0];
        }

        public unsafe string? GetManaged()
        {
            fixed (byte* ptr = _buffer[.._size]) {
                return Utf8StringMarshaller.ConvertToManaged(ptr)?[.._size];
            }
        }
    }

    public Enumerator GetEnumerator()
        => new(this);

    public ref struct Enumerator
    {
        private readonly Span<byte> _buffer;
        private readonly Span<LabelGroup> _groups;
        private readonly int _groupCount;
        private int _groupIndex;
        private int _labelIndex;
        private int _position;

        public Enumerator(LabelSectionReader labelSectionReader)
        {
            SpanReader reader = SpanReader.Native(_buffer = labelSectionReader._buffer);
            _groupCount = reader.Read<int>();
            _groups = reader.ReadSpan<LabelGroup, LabelGroup.Reverser>(_groupCount);
            _position = reader.Position;
        }

        public LabelMarshal Current {
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