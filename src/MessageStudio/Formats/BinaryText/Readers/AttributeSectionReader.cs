using MessageStudio.Formats.BinaryText.Structures;
using MessageStudio.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace MessageStudio.Formats.BinaryText.Readers;

public readonly ref struct AttributeSectionReader
{
    private readonly Span<byte> _buffer;
    private readonly Span<int> _offsets;
    private readonly TextEncoding _encoding;
    private readonly int _attributeSize;
    private readonly int _count;

    public readonly AttributeMarshal this[int index] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            if (_count == 0 || _attributeSize == 0) {
                return default;
            }

            if (index >= _count) {
                throw new IndexOutOfRangeException($"Label at position '{index}' does not exist in Label['{_count}']");
            }

            int offset = _offsets[index];
            int endOffset = ++index >= _count ? _buffer.Length : _offsets[index];
            return new(_buffer[offset..endOffset], _encoding);
        }
    }

    public AttributeSectionReader(ref SpanReader reader, ref MsbtSectionHeader header, TextEncoding encoding)
    {
        _encoding = encoding;

        int sectionOffset = reader.Position;
        _count = reader.Read<int>();
        _attributeSize = reader.Read<int>();

        if (_attributeSize == 0) {
            return;
        }

        if (_attributeSize != 4) {
            throw new NotSupportedException("Only uint32 and nullptr attribute offsets are supported");
        }

        _offsets = reader.ReadSpan<int>(_count);

        if (reader.IsNotSystemByteOrder()) {
            if (encoding == TextEncoding.Unicode) {
                int eos = sectionOffset + header.SectionSize;
                while (reader.Position < eos) {
                    reader.Reverse<ushort>();
                }
            }
        }

        _buffer = reader.Read(header.SectionSize, sectionOffset);
    }

    public readonly ref struct AttributeMarshal(Span<byte> buffer, TextEncoding encoding)
    {
        public readonly Span<byte> Buffer = buffer;
        public readonly TextEncoding Encoding = encoding;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<char> GetUnicode()
        {
            return MemoryMarshal.Cast<byte, char>(Buffer);
        }

        public unsafe readonly string? GetManaged()
        {
            if (Buffer.IsEmpty) {
                return null;
            }

            if (Encoding == TextEncoding.UTF8) {
                fixed (byte* ptr = Buffer) {
                    return Utf8StringMarshaller.ConvertToManaged(ptr);
                };
            }
            else {
                fixed (ushort* ptr = MemoryMarshal.Cast<byte, ushort>(Buffer)) {
                    return Utf16StringMarshaller.ConvertToManaged(ptr);
                };
            }
        }
    }
}
