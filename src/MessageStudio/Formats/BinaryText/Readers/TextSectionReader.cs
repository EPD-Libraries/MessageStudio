using MessageStudio.Common;
using MessageStudio.Formats.BinaryText.Structures;
using Revrs;
using System.Runtime.CompilerServices;

namespace MessageStudio.Formats.BinaryText.Readers;

public readonly ref struct TextSectionReader
{
    private readonly Span<byte> _buffer;
    private readonly Span<int> _offsets;
    private readonly TextEncoding _encoding;
    private readonly int _count;

    public readonly MsbtText this[int index] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            if (index >= _count) {
                throw new IndexOutOfRangeException($"Label at position '{index}' does not exist in Label['{_count}']");
            }

            int offset = _offsets[index];
            int endOffset = ++index >= _count ? _buffer.Length : _offsets[index];
            return new(_buffer[offset..endOffset], _encoding);
        }
    }

    public TextSectionReader(ref RevrsReader reader, ref MsbtSectionHeader header, TextEncoding encoding)
    {
        _encoding = encoding;

        int sectionOffset = reader.Position;
        _count = reader.Read<int>();
        _offsets = reader.ReadSpan<int>(_count);

        if (reader.Endianness.IsNotSystemEndianness()) {
            if (encoding == TextEncoding.Unicode) {
                int eos = sectionOffset + header.SectionSize;
                while (reader.Position < eos) {
                    reader.Reverse<ushort>();
                }
            }
        }

        _buffer = reader.Read(header.SectionSize, sectionOffset);
    }
}
