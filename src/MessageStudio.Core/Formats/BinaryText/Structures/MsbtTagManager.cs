using MessageStudio.Core.Formats.BinaryText.Structures.Tags;

namespace MessageStudio.Core.Formats.BinaryText.Structures;

public static class MsbtTagManager
{
    public static IMsbtTag FromBinary(in ushort group, in ushort type, in Span<ushort> data)
    {
        return UnknownTag.FromBinary(group, type, data);
    }

    public static IMsbtTag FromText(in ReadOnlySpan<char> text)
    {
        return UnknownTag.FromText(text);
    }
}
