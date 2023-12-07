using MessageStudio.Formats.BinaryText.Extensions;
using MessageStudio.Formats.BinaryText.Structures;
using MessageStudio.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using StringBuilder = System.Text.StringBuilder;

namespace MessageStudio.Formats.BinaryText.Readers;

public readonly ref struct TextSectionReader
{
    private readonly Span<byte> _buffer;
    private readonly Span<int> _offsets;
    private readonly Encoding _encoding;
    private readonly int _count;

    public readonly TextMarshal this[int index] {
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

    public TextSectionReader(ref SpanReader reader, ref MsbtSectionHeader header, Encoding encoding)
    {
        _encoding = encoding;

        int sectionOffset = reader.Position;
        _count = reader.Read<int>();
        _offsets = reader.ReadSpan<int>(_count);

        if (reader.IsNotSystemByteOrder()) {
            if (encoding == Encoding.Unicode) {
                int eos = sectionOffset + header.SectionSize;
                while (reader.Position < eos) {
                    reader.Reverse<ushort>();
                }
            }
        }

        _buffer = reader.Read(header.SectionSize, sectionOffset);
    }

    public readonly ref struct TextMarshal(Span<byte> buffer, Encoding encoding)
    {
        public readonly Span<byte> Buffer = buffer;
        public readonly Encoding Encoding = encoding;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<char> GetUnicode()
        {
            return MemoryMarshal.Cast<byte, char>(Buffer[..^2]);
        }

        public readonly string? GetManaged()
        {
            StringBuilder sb = new();
            if (Encoding == Encoding.UTF8) {
                WriteUtf8(sb);
            }
            else {
                WriteUtf16(sb);
            }

            return sb.ToString();
        }

        private readonly void WriteUtf8(in StringBuilder sb)
        {
            for (int i = 0; i < Buffer.Length; i++) {
                byte value = Buffer[i];
                if (value == 0xE) {
                    byte group = Buffer[++i];
                    byte type = Buffer[++i];
                    byte size = Buffer[++i];
                    sb.WriteTag(group, type, Buffer[++i..(i += size)]);
                }
                else if (value == 0xF) {
                    byte group = Buffer[++i];
                    byte type = Buffer[++i];
                    sb.WriteEndTag(group, type);
                }
                else if (value == 0x0) {
                    continue;
                }
                else {
                    sb.Append((char)value);
                }
            }
        }

        private readonly void WriteUtf16(in StringBuilder sb)
        {
            Span<ushort> buffer = MemoryMarshal.Cast<byte, ushort>(Buffer);
            for (int i = 0; i < buffer.Length; i++) {
                ushort value = buffer[i];
                if (value == 0xE) {
                    ushort group = buffer[++i];
                    ushort type = buffer[++i];
                    ushort size = buffer[++i];
                    sb.WriteTag(group, type, MemoryMarshal.Cast<ushort, byte>(buffer[++i..(i += size / 2)]));
                    i--;
                }
                else if (value == 0xF) {
                    ushort group = buffer[++i];
                    ushort type = buffer[++i];
                    sb.WriteEndTag(group, type);
                }
                else if (value == 0x0) {
                    continue;
                }
                else {
                    sb.Append((char)value);
                }
            }
        }
    }
}
