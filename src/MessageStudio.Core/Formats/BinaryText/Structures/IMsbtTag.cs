using System.Text;

namespace MessageStudio.Core.Formats.BinaryText.Structures;

public interface IMsbtTag
{
    public static abstract IMsbtTag FromBinary(in ushort group, in ushort type, in Span<ushort> data);
    public static abstract IMsbtTag FromText(ReadOnlySpan<char> text);
    public byte[] ToBinary();
    public void ToText(ref StringBuilder sb);
    public string ToText()
    {
        StringBuilder sb = new();
        ToText(ref sb);
        return sb.ToString();
    }
}
