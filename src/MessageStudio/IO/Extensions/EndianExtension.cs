using System.Runtime.CompilerServices;

namespace MessageStudio.IO.Extensions;

internal static class EndianExtension
{
    /// <summary>
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the system <see cref="Endian"/> does not match the provided <paramref name="endianness"/>
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSystemByteOrder(this Endian endianness)
        => !IsNotSystemByteOrder(endianness);

    /// <summary>
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the system <see cref="Endian"/> does not match the provided <paramref name="endianness"/>
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNotSystemByteOrder(this Endian endianness)
        => endianness == Endian.Big && BitConverter.IsLittleEndian || endianness == Endian.Little && !BitConverter.IsLittleEndian;
}
