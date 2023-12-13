using MessageStudio.Common;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace MessageStudio.Formats.BinaryText.Structures;

public readonly ref struct MsbtAttribute(Span<byte> buffer, TextEncoding encoding)
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