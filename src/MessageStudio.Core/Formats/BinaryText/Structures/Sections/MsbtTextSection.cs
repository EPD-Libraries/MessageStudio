using MessageStudio.Core.Common;
using MessageStudio.Core.Formats.BinaryText.Structures.Common;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.InteropServices;

namespace MessageStudio.Core.Formats.BinaryText.Structures.Sections;

public readonly ref struct MsbtTextSection
{
    private readonly Endian _endianness;
    private readonly Span<byte> _tableBuffer;

    public readonly int Count;

    public MsbtTextSection(ref Parser parser)
    {
        _endianness = parser.Endian;

        SectionHeader header = parser.ReadStruct<SectionHeader>();
        int tableOffset = parser.Position;

        Count = parser.Read<int>();
        _tableBuffer = parser.ReadSpan(header.Size, tableOffset);
        parser.Align(0x10);
    }

    public unsafe readonly struct MsbtText(int index, ushort* valuePtr)
    {
        private readonly ushort* _valuePtr = valuePtr;

        public readonly int Index = index;

        public string Value {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Utf16StringMarshaller.ConvertToManaged(_valuePtr)
                ?? "";
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator() => new(this);

    public ref struct Enumerator
    {
        private readonly Parser _parser;
        private readonly MsbtTextSection _section;
        private readonly Span<ushort> _strings;
        private readonly int _offsetsOffset;
        private readonly int _stringsOffset;
        private int _index = -1;

        public Enumerator(MsbtTextSection section)
        {
            _parser = new(section._tableBuffer, section._endianness);
            _offsetsOffset = sizeof(uint);
            _stringsOffset = _offsetsOffset + section.Count * sizeof(uint);
            _strings = MemoryMarshal.Cast<byte, ushort>(section._tableBuffer[_stringsOffset..section._tableBuffer.Length]);

            if (_parser.IsNotSystemByteOrder()) {
                for (int i = 0; i < _strings.Length; i++) {
                    _strings[i] = BinaryPrimitives.ReverseEndianness(_strings[i]);
                }
            }

            _section = section;
        }

        public readonly unsafe MsbtText Current {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                int offset = (_parser.Read<int>(_offsetsOffset + (sizeof(uint) * _index)) - _stringsOffset) / 2;
                fixed (ushort* ptr = _strings[offset..]) {
                    return new(_index, ptr);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() => ++_index < _section.Count;
    }
}
