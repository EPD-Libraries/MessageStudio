using MessageStudio.IO.Extensions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MessageStudio.IO;

public enum Endian : ushort
{
    Big = 0xFEFF,
    Little = 0xFFFE,
}

public ref struct SpanReader(Span<byte> buffer, Endian endianness = Endian.Big)
{
    private readonly Span<byte> _buffer = buffer;

    private int _position = 0;
    public readonly int Position {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _position;
    }

    public readonly int Length {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _buffer.Length;
    }

    private Endian _endianness = endianness;
    public Endian Endianness {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _endianness;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _endianness = value;
    }

    /// <summary>
    /// Create a new system native <see cref="SpanReader"/> over a span of memory
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SpanReader Native(Span<byte> buffer)
        => new(buffer, BitConverter.IsLittleEndian ? Endian.Little : Endian.Big);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Seek(int position)
    {
        _position = position;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Move(int length)
    {
        _position += length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Align(int size)
    {
        _position += (size - _position % size) % size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<byte> Read(int length, int offset = -1)
    {
        int rOffset = ResolveOffset(offset);

        if (rOffset + length <= _buffer.Length) {
            _position = rOffset + length;
            return _buffer[rOffset..(rOffset + length)];
        }

        throw EoF();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T Read<T>(int offset = -1) where T : unmanaged
    {
        int rOffset = ResolveOffset(offset);
        Span<byte> buffer = _buffer[rOffset..(_position = rOffset + Unsafe.SizeOf<T>())];

        if (buffer.Length > 1 && IsNotSystemByteOrder()) {
            buffer.Reverse();
            return ref MemoryMarshal.Cast<byte, T>(buffer)[0];
        }

        return ref MemoryMarshal.Cast<byte, T>(buffer)[0];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reverse<T>(int offset = -1) where T : unmanaged
    {
        int rOffset = ResolveOffset(offset);
        _buffer[rOffset..(_position = rOffset + Unsafe.SizeOf<T>())].Reverse();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T Read<T, TReverser>(int offset = -1) where T : unmanaged where TReverser : ISpanReverser
    {
        int rOffset = ResolveOffset(offset);
        Span<byte> buffer = _buffer[rOffset..(_position = rOffset + Unsafe.SizeOf<T>())];

        if (buffer.Length > 1 && IsNotSystemByteOrder()) {
            TReverser.Reverse(buffer);
            return ref MemoryMarshal.Cast<byte, T>(buffer)[0];
        }

        return ref MemoryMarshal.Cast<byte, T>(buffer)[0];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reverse<T, TReverser>(int offset = -1) where T : unmanaged where TReverser : ISpanReverser
    {
        int rOffset = ResolveOffset(offset);
        TReverser.Reverse(_buffer[rOffset..(_position = rOffset + Unsafe.SizeOf<T>())]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> ReadSpan<T>(int length, int offset = -1) where T : unmanaged
    {
        int rOffset = ResolveOffset(offset);
        int blockSize = Unsafe.SizeOf<T>();
        int bufferSize = blockSize * length;

        if (rOffset + bufferSize <= _buffer.Length) {
            _position = rOffset + bufferSize;

            if (blockSize > 1 && IsNotSystemByteOrder()) {
                Span<byte> buffer = _buffer[rOffset..(rOffset + bufferSize)];
                ReverseSpanBuffer(buffer, blockSize, length);
                return MemoryMarshal.Cast<byte, T>(buffer);
            }

            return MemoryMarshal.Cast<byte, T>(_buffer[rOffset..(rOffset + bufferSize)]);
        }

        throw EoF();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> ReadSpan<T, TReverser>(int length, int offset = -1) where T : struct where TReverser : ISpanReverser
    {
        int rOffset = ResolveOffset(offset);
        int blockSize = Unsafe.SizeOf<T>();
        int bufferSize = blockSize * length;

        if (rOffset + bufferSize <= _buffer.Length) {
            _position = rOffset + bufferSize;

            if (blockSize > 1 && IsNotSystemByteOrder()) {
                Span<byte> buffer = _buffer[rOffset..(rOffset + bufferSize)];
                ReverseStructSpanBuffer<TReverser>(buffer, blockSize, length);
                return MemoryMarshal.Cast<byte, T>(buffer);
            }

            return MemoryMarshal.Cast<byte, T>(_buffer[rOffset..(rOffset + bufferSize)]);
        }

        throw EoF();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReverseSpan<T>(int length, int offset = -1) where T : struct
    {
        int rOffset = ResolveOffset(offset);
        int blockSize = Unsafe.SizeOf<T>();
        ReverseSpanBuffer(_buffer[rOffset..(_position = rOffset + blockSize * length)], blockSize, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReverseSpan<T, TReverser>(int length, int offset = -1) where T : struct where TReverser : ISpanReverser
    {
        int rOffset = ResolveOffset(offset);
        int blockSize = Unsafe.SizeOf<T>();
        ReverseStructSpanBuffer<TReverser>(_buffer[rOffset..(_position = rOffset + blockSize * length)], blockSize, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ReverseSpanBuffer(in Span<byte> buffer, int blockSize, int blockChainLength)
    {
        for (int i = 0; i < blockChainLength; i++) {
            int blockOffset = blockSize * i;
            buffer[blockOffset..(blockOffset + blockSize)].Reverse();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ReverseStructSpanBuffer<TReverser>(in Span<byte> buffer, int blockSize, int blockChainLength) where TReverser : ISpanReverser
    {
        for (int i = 0; i < blockChainLength; i++) {
            int blockOffset = blockSize * i;
            TReverser.Reverse(buffer[blockOffset..(blockOffset + blockSize)]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static InvalidOperationException EoF()
        => new("The requested buffer is larger than the source buffer");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly int ResolveOffset(in int optionalOffset)
        => optionalOffset < 0 ? _position : optionalOffset;

    /// <inheritdoc cref="EndianExtension.IsNotSystemByteOrder(Endian)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool IsNotSystemByteOrder()
        => !_endianness.IsSystemByteOrder();
}
