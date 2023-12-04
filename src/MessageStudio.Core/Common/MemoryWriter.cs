using MessageStudio.Core.Common.Extensions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace MessageStudio.Core.Common;

public class MemoryWriter : IDisposable
{
    private readonly Stream _stream;
    private readonly Endian _endianness;

    public MemoryWriter(in Stream stream, Endian endianness)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Seek(int position)
    {
        _stream.Seek(position, SeekOrigin.Begin);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Move(int length)
    {
        _stream.Seek(length, SeekOrigin.Current);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Align(int value)
    {
        _stream.Seek((value - _stream.Position % value) % value, SeekOrigin.Current);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write<T>(T value) where T : unmanaged
    {
        int blockSize = Unsafe.SizeOf<T>();
        Span<byte> buffer = blockSize <= 0xF0000
            ? stackalloc byte[blockSize] : new byte[blockSize];

        MemoryMarshal.Write(buffer, value);

        if (_endianness.IsNotSystemByteOrder()) {
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
        Span<ushort> buffer = new(ptr, value.Length);
        _stream.Write(MemoryMarshal.Cast<ushort, byte>(buffer));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteStruct<T>(T value) where T : struct, IReversable
    {
        int blockSize = Unsafe.SizeOf<T>();
        Span<byte> buffer = blockSize <= 0xF0000
            ? stackalloc byte[blockSize] : new byte[blockSize];

        MemoryMarshal.Write(buffer, value);

        if (_endianness.IsNotSystemByteOrder()) {
            T.Reverse(buffer);
        }

        _stream.Write(buffer);
    }

    public void Dispose()
    {
        _stream.Dispose();
        GC.SuppressFinalize(this);
    }
}
