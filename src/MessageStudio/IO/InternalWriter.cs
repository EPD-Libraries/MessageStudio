using MessageStudio.IO.Extensions;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace MessageStudio.IO;

internal class InternalWriter : IDisposable
{
    private readonly Stream _stream;
    private readonly Endian _endianness;

    public InternalWriter(in Stream stream, Endian endianness)
    {
        if (!stream.CanWrite) {
            throw new InvalidOperationException("The input stream must be writable");
        }

        if (!stream.CanSeek) {
            throw new InvalidOperationException("The input stream must be seekable");
        }

        _endianness = endianness;
        _stream = stream;
    }

    public long Position {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _stream.Position;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Seek(long position)
    {
        _stream.Seek(position, SeekOrigin.Begin);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Move(long length)
    {
        _stream.Seek(length, SeekOrigin.Current);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Align(long value)
    {
        _stream.Seek((value - _stream.Position % value) % value, SeekOrigin.Current);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ReadOnlySpan<byte> buffer)
    {
        _stream.Write(buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write<T>(in T value) where T : unmanaged
    {
        int blockSize = Unsafe.SizeOf<T>();
        Span<byte> buffer = blockSize <= 0xF0000
            ? stackalloc byte[blockSize] : new byte[blockSize];

        MemoryMarshal.Write(buffer, value);

        if (IsNotSystemByteOrder()) {
            buffer.Reverse();
        }

        _stream.Write(buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void WriteUtf8String(string value)
    {
        byte* ptr = Utf8StringMarshaller.ConvertToUnmanaged(value);
        Span<byte> buffer = new(ptr, value.Length);
        _stream.Write(buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void WriteUtf16String(string value)
    {
        ushort* ptr = Utf16StringMarshaller.ConvertToUnmanaged(value);
        if (IsNotSystemByteOrder()) {
            for (int i = 0; i < value.Length; i++) {
                ptr[i] = BinaryPrimitives.ReverseEndianness(ptr[i]);
            }
        }

        Span<ushort> buffer = new(ptr, value.Length);
        _stream.Write(MemoryMarshal.Cast<ushort, byte>(buffer));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write<T, TReverser>(in T value) where T : struct where TReverser : ISpanReverser
    {
        int blockSize = Unsafe.SizeOf<T>();
        Span<byte> buffer = blockSize <= 0xF0000
            ? stackalloc byte[blockSize] : new byte[blockSize];

        MemoryMarshal.Write(buffer, value);

        if (_endianness.IsNotSystemByteOrder()) {
            TReverser.Reverse(buffer);
        }

        _stream.Write(buffer);
    }

    /// <inheritdoc cref="EndianExtension.IsNotSystemByteOrder(Endian)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsNotSystemByteOrder()
        => !_endianness.IsSystemByteOrder();

    public void Dispose()
    {
        _stream.Dispose();
        GC.SuppressFinalize(this);
    }
}
