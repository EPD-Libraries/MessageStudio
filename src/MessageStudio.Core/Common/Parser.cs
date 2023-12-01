using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MessageStudio.Core.Common;

public enum Endian : ushort
{
    Big = 0xFFFE,
    Little = 0xFEFF,
}

public ref struct Parser(ReadOnlySpan<byte> buffer)
{
    private readonly ReadOnlySpan<byte> _buffer = buffer;
    private int _position = 0;

    public Endian Endian = Endian.Big;

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
    public unsafe T Read<T>(in int offset = -1) where T : struct
    {
        int rOffset = ResolveOffset(offset);
        T result = MemoryMarshal.Read<T>(_buffer[rOffset..]);
        _position = rOffset + sizeof(T);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> ReadSpan(int length, int offset = -1)
    {
        int rOffset = ResolveOffset(offset);

        if (rOffset + length < _buffer.Length) {
            _position = rOffset + length;
            return _buffer[rOffset..(rOffset + length)];
        }

        throw EoF();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe ReadOnlySpan<T> ReadSpan<T>(int length, int offset = -1) where T : struct
    {
        int rOffset = ResolveOffset(offset);
        int bufferSize = sizeof(T) * length;

        if (rOffset + bufferSize <= _buffer.Length) {
            _position = rOffset + bufferSize;
            return MemoryMarshal.Cast<byte, T>(_buffer[rOffset..(rOffset + bufferSize)]);
        }

        throw EoF();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CheckForMagic(in ReadOnlySpan<byte> magic, int offset = -1)
        => ReadSpan(magic.Length, offset).SequenceEqual(magic);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static InvalidOperationException EoF()
        => new("The requested buffer is larger than the source buffer");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly int ResolveOffset(in int optionalOffset)
        => optionalOffset < 0 ? _position : optionalOffset;
}
