using System.Runtime.CompilerServices;

namespace MessageStudio.IO;

/// <summary>
/// Static byte reversal interface for byte re-ordering
/// </summary>
public interface ISpanReverser
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static abstract void Reverse(in Span<byte> buffer);
}
