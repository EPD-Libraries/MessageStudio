using MessageStudio.Common;
using MessageStudio.Formats.BinaryText.Extensions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace MessageStudio.Formats.BinaryText.Structures;

public readonly ref struct MsbtText(Span<byte> buffer, TextEncoding encoding)
{
    public readonly Span<byte> Buffer = buffer;
    public readonly TextEncoding Encoding = encoding;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<char> GetUnicode()
    {
        return MemoryMarshal.Cast<byte, char>(Buffer[..^2]);
    }

    public readonly string? GetManaged()
    {
        StringBuilder sb = new();
        if (Encoding == TextEncoding.UTF8) {
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
                sb.AppendFunction(group, type, Buffer[++i..(i += size)], TextEncoding.UTF8);
            }
            else if (value == 0xF) {
                byte group = Buffer[++i];
                byte type = Buffer[++i];
                sb.AppendEmptyFunction(group, type);
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
                sb.AppendFunction(group, type, MemoryMarshal.Cast<ushort, byte>(buffer[++i..(i += size / 2)]), TextEncoding.Unicode);
                i--;
            }
            else if (value == 0xF) {
                ushort group = buffer[++i];
                ushort type = buffer[++i];
                sb.AppendEmptyFunction(group, type);
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