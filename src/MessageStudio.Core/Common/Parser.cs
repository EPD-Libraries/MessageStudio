using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MessageStudio.Core.Common;

public enum Endian : ushort
{
    Big = 0xFEFF,
    Little = 0xFFFE,
}

public ref struct Parser(Span<byte> buffer, Endian endian = Endian.Big)
{
    private readonly Span<byte> _buffer = buffer;
    private int _position = 0;

    public Endian Endian = endian;

    public readonly int Position {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _position;
    }

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
    public T Read<T>(int offset = -1) where T : unmanaged
    {
        int rOffset = ResolveOffset(offset);
        Span<byte> buffer = _buffer[rOffset..(_position = rOffset + Unsafe.SizeOf<T>())];

        if (buffer.Length > 1 && IsNotSystemByteOrder()) {
            buffer.Reverse();
            T result = MemoryMarshal.Read<T>(buffer);
            buffer.Reverse();
            return result;
        }

        return MemoryMarshal.Read<T>(buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe T ReadStruct<T>(int offset = -1) where T : struct, IReversable
    {
        int rOffset = ResolveOffset(offset);
        Span<byte> buffer = _buffer[rOffset..(_position = rOffset + sizeof(T))];

        if (buffer.Length > 1 && IsNotSystemByteOrder()) {
            T.Reverse(buffer);
            T result = MemoryMarshal.Read<T>(buffer);
            T.Reverse(buffer);
            return result;
        }

        return MemoryMarshal.Read<T>(buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<byte> ReadSpan(int length, int offset = -1)
    {
        int rOffset = ResolveOffset(offset);

        if (rOffset + length <= _buffer.Length) {
            _position = rOffset + length;
            return _buffer[rOffset..(rOffset + length)];
        }

        throw EoF();
    }

    /// <summary>
    /// <para>Reads a span of <typeparamref name="T"/> from the buffer, reversing the bytes into a copied buffer if necessary.</para>
    /// <b>Warning: this function allocates memory when reading the oposite byte-order, avoid using if another solution is faster.</b>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="length"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> ReadSpan<T>(int length, int offset = -1) where T : struct, IReversable
    {
        int rOffset = ResolveOffset(offset);
        int blockSize = Unsafe.SizeOf<T>();
        int bufferSize = blockSize * length;

        if (rOffset + bufferSize <= _buffer.Length) {
            _position = rOffset + bufferSize;

            if (blockSize > 1 && IsNotSystemByteOrder()) {
                Span<byte> buffer = new byte[bufferSize];
                _buffer[rOffset..(rOffset + bufferSize)].CopyTo(buffer);
                ReverseStructSpanBuffer<T>(buffer, blockSize, length);
                return MemoryMarshal.Cast<byte, T>(buffer);
            }

            return MemoryMarshal.Cast<byte, T>(_buffer[rOffset..(rOffset + bufferSize)]);
        }

        throw EoF();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ReverseStructSpanBuffer<T>(in Span<byte> buffer, int blockSize, int blockChainLength) where T : struct, IReversable
    {
        for (int i = 0; i < blockChainLength; i++) {
            int blockOffset = blockSize * i;
            T.Reverse(buffer[blockOffset..(blockOffset + blockSize)]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static InvalidOperationException EoF()
        => new("The requested buffer is larger than the source buffer");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly int ResolveOffset(in int optionalOffset)
        => optionalOffset < 0 ? _position : optionalOffset;

    /// <summary>
    /// Returns true or false based on the OS byte-order and read endianness
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool IsNotSystemByteOrder()
        => Endian == Endian.Big && BitConverter.IsLittleEndian || Endian == Endian.Little && !BitConverter.IsLittleEndian;
}
