using MessageStudio.Core.Formats.BinaryText.Structures.Tags;

namespace MessageStudio.Core.Formats.BinaryText.Structures;

public static class MsbtTagManager
{
    public static IMsbtTag FromBinary(in ushort group, in ushort type, in Span<ushort> data)
    {
        throw new NotImplementedException();
    }
}
