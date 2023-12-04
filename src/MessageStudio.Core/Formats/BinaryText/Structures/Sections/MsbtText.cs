using MessageStudio.Core.Common;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

namespace MessageStudio.Core.Formats.BinaryText.Structures.Sections;

public unsafe class MsbtText(int index, byte* valuePtr, int valueLength, Encoding encoding, Endian endianness)
{
    private readonly Encoding _encoding = encoding;
    private readonly Endian _endianness = endianness;

    private readonly byte* _valuePtr = valuePtr;
    private readonly int _valueLength = valueLength;
    private string? _value = null;

    public int Index { get; } = index;
    public int Length => _valueLength;

    public string Value {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _value ??= GetManaged();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string GetManaged()
    {
        StringBuilder sb = new();
        if (_encoding == Encoding.UTF8) {
            throw new NotSupportedException("UTF8 encoding is not supported");
        }
        else {
            Span<ushort> buffer = new(_valuePtr, _valueLength / 2);
            UnicodeReader reader = new(buffer, _endianness);
            for (int i = 0; i < buffer.Length; i++) {
                if (buffer[i] == 0x0) {
                    // Continue before any byte-reversal
                    continue;
                }

                ushort value = reader.Read(i);

                if (value == 0xE) {
                    ushort tagGroup = reader.Read(++i);
                    ushort tagType = reader.Read(++i);
                    ushort tagSize = reader.Read(++i);

                    i++;
                    IMsbtTag tag = MsbtTagManager.FromBinary(tagGroup, tagType, reader.ReadSpan(ref i, tagSize / 2));
                    tag.ToText(ref sb);
                    i--;
                }
                else if (value == 0xF) {
                    throw new NotSupportedException("0xF tags are not yet supported");
                }
                else {
                    sb.Append((char)value);
                }
            }
        }

        return sb.ToString();
    }

    private readonly ref struct UnicodeReader(Span<ushort> buffer, Endian endianness)
    {
        private readonly Span<ushort> _buffer = buffer;
        private readonly Endian _endianness = endianness;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort Read(in int index)
        {
            return MemoryReader.IsNotSystemByteOrder(_endianness)
                ? BinaryPrimitives.ReverseEndianness(_buffer[index]) : _buffer[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<ushort> ReadSpan(ref int startIndex, in int length)
        {
            int relativeLength = startIndex + length;
            if (MemoryReader.IsNotSystemByteOrder(_endianness)) {
                for (int i = startIndex; i < relativeLength; i++) {
                    _buffer[i] = BinaryPrimitives.ReverseEndianness(_buffer[i]);
                }
            }

            Span<ushort> result = _buffer[startIndex..relativeLength];
            startIndex += length;
            return result;
        }
    }
}