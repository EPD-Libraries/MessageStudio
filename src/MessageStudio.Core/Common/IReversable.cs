using System.Runtime.CompilerServices;

namespace MessageStudio.Core.Common;

public interface IReversable
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static abstract void Reverse(in Span<byte> buffer);
}
