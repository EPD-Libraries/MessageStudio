using MessageStudio.Formats.BinaryText.Extensions;
using Revrs;
using System.Text;

namespace MessageStudio.Formats.BinaryText.Components;

public class DefaultTagResolver : ITagResolver
{
    private static readonly Lazy<DefaultTagResolver> _shared = new(() => new());
    public static DefaultTagResolver Shared => _shared.Value;

    public string GetName(ushort group, ushort type)
    {
        return $"{group}|{type}";
    }

    public (ushort, ushort)? GetGroupAndType(ReadOnlySpan<char> name)
    {
        int split = name.IndexOf('|');
        return (ushort.Parse(name[..split]), ushort.Parse(name[++split..]));
    }

    public bool WriteBinaryUtf8(RevrsWriter writer, ReadOnlySpan<char> name, in TagParams @params)
    {
        throw new NotImplementedException();
    }

    public bool WriteBinaryUtf16(RevrsWriter writer, ReadOnlySpan<char> name, in TagParams @params)
    {
        ReadOnlySpan<char> data = @params["Data"];
        byte[] buffer = data.IsEmpty ? [] : Convert.FromHexString(data[2..]);
        writer.Write((ushort)buffer.Length);

        if (!data.IsEmpty) {
            writer.Write(buffer);
            writer.Align(2);
        }

        return true;
    }

    public void WriteText(StringBuilder sb, ReadOnlySpan<char> name, Span<byte> data)
    {
        if (!data.IsEmpty) {
            sb.OpenParam("Data");
            sb.Append("0x");
            sb.Append(Convert.ToHexString(data));
            sb.CloseParam();
        }
    }
}
