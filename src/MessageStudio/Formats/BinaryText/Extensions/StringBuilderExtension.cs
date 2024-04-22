using CommunityToolkit.HighPerformance.Buffers;
using System.Text;

namespace MessageStudio.Formats.BinaryText.Extensions;

public static class StringBuilderExtension
{
    public static void AppendHex(this StringBuilder sb, ReadOnlySpan<byte> data)
    {
        using SpanOwner<char> hex = SpanOwner<char>.Allocate(2 + data.Length * 2);
        hex.Span[0] = '0';
        hex.Span[1] = 'x';
        for (int i = 0; i < data.Length; i++) {
            data[i].TryFormat(hex.Span[(2 + i * 2)..(4 + i * 2)], out _, "x2");
        }

        sb.Append(hex.Span);
    }
}
