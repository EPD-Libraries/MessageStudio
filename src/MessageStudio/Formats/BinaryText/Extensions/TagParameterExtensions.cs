using System.Text;

namespace MessageStudio.Formats.BinaryText.Extensions;

public static class TagParameterExtensions
{
    public static void OpenParam(this StringBuilder sb, string name)
    {
        sb.Append(' ');
        sb.Append(name);
        sb.Append("='");
    }

    public static void CloseParam(this StringBuilder sb)
    {
        sb.Append('\'');
    }
}
