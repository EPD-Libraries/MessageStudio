using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace MessageStudio.Formats.BinaryText.Structures;

public readonly ref struct MsbtLabel(byte size, Span<byte> buffer)
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