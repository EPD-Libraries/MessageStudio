using MessageStudio.Common;
using MessageStudio.Formats.BinaryText.Components;
using Revrs;
using System.Text;

namespace MessageStudio.Formats.BinaryText.Extensions;

internal static class FunctionMapExtension
{
    internal static void AppendFunction(this StringBuilder sb, ushort group, ushort type, Span<byte> data, TextEncoding encoding)
    {
        FunctionMap.Current.AppendFunction(sb, group, type, data, encoding);
    }

    internal static void AppendEmptyFunction(this StringBuilder sb, ushort group, ushort type)
    {
        FunctionMap.Current.AppendEmptyFunction(sb, group, type);
    }

    internal static void WriteFunction(this ref RevrsWriter writer, ReadOnlySpan<char> text, TextEncoding encoding)
    {
        FunctionMap.Current.WriteFunctionParams(ref writer, text, encoding);
    }
}
