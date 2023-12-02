using MessageStudio.Core.Common;
using MessageStudio.Core.Formats.BinaryText.Structures.Common;
using System.Buffers.Binary;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace MessageStudio.Core.Formats.BinaryText.Structures.Sections;

public readonly ref struct MsbtAttributeSection
{
    private readonly Endian _endianness;
    private readonly int _attributeSize;
    private readonly Span<byte> _attributeBuffer;

    public readonly int Count;

    public MsbtAttributeSection(ref Parser parser)
    {
        _endianness = parser.Endian;

        SectionHeader header = parser.ReadStruct<SectionHeader>();
        int tableOffset = parser.Position;

        Count = parser.Read<int>();
        _attributeSize = parser.Read<int>();

        if (_attributeSize != 4) {
            throw new NotSupportedException("Only UINT32 attribute offsets are supported");
        }

        _attributeBuffer = parser.ReadSpan(header.Size, tableOffset);
        parser.Align(0x10);
    }

    public unsafe readonly struct MsbtAttribute(int index, ushort* valuePtr)
    {
        private readonly ushort* _valuePtr = valuePtr;

        public readonly int Index = index;

        public string Value {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Utf16StringMarshaller.ConvertToManaged(_valuePtr)
                ?? string.Empty;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator() => new(this);

    public ref struct Enumerator
    {
        private readonly Parser _parser;
        private readonly MsbtAttributeSection _section;
        private readonly Span<ushort> _strings;
        private readonly int _offsetsOffset;
        private readonly int _stringsOffset;
        private int _index = -1;

        public Enumerator(MsbtAttributeSection section)
        {
            _parser = new(section._attributeBuffer, section._endianness);
            _offsetsOffset = sizeof(uint) + sizeof(uint);
            _stringsOffset = _offsetsOffset + section.Count * section._attributeSize;

            if (_parser.IsNotSystemByteOrder()) {
                int blockSize = section._attributeBuffer.Length - _stringsOffset;
                int utf16Length = blockSize / 2;
                int currentOffset = _stringsOffset;
                for (int i = 0; i < utf16Length; i++) {
                    section._attributeBuffer[currentOffset..(currentOffset += 2)].Reverse();
                }
            }

            _strings = MemoryMarshal.Cast<byte, ushort>(section._attributeBuffer[_stringsOffset..section._attributeBuffer.Length]);
            _section = section;
            _parser.Seek(_offsetsOffset);
        }

        public readonly unsafe MsbtAttribute Current {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                int offset = (_parser.Read<int>(_offsetsOffset + (_section._attributeSize * _index)) - _stringsOffset) / 2;
                fixed (ushort* ptr = _strings[offset..]) {
                    return new(_index, ptr);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() => ++_index < _section.Count;
    }
}
