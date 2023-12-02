namespace MessageStudio.Core.Common;

public interface IReversable
{
    public static abstract void Reverse(in Span<byte> buffer);
}
