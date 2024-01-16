using MessageStudio.Common;
using MessageStudio.Formats.BinaryText.Components;
using Revrs;
using System.Runtime.CompilerServices;
using System.Text;

namespace MessageStudio.Formats.BinaryText.Extensions;

public static class TagResolverExtensions
{
    public static ITagResolver Resolver { get; private set; } = new DefaultTagResolver();

    /// <summary>
    /// Register the provided <paramref name="resolver"/> to be used when resolving tags.
    /// </summary>
    /// <param name="resolver"></param>
    public static void Register(this ITagResolver resolver)
    {
        Resolver = resolver;
    }

    internal static void WriteTag(this StringBuilder sb, ushort group, ushort type, Span<byte> data)
    {
        sb.Append('<');

        if (Resolver.GetName(group, type) is string name) {
            sb.Append(name);
            Resolver.WriteText(sb, group, type, data);
        }
        else {
            name = DefaultTagResolver.Shared.GetName(group, type);
            sb.Append(name);
            DefaultTagResolver.Shared.WriteText(sb, group, type, data);
        }

        sb.Append("/>");
    }

    internal static void WriteTag(this RevrsWriter writer, ReadOnlySpan<char> text, TextEncoding encoding)
    {
        ReadOnlySpan<char> name = text.ReadTagName(out int paramsStartIndex);
        TagParams @params = new(text[paramsStartIndex..]);

        (ushort group, ushort type) = Resolver.GetGroupAndType(name)
            ?? DefaultTagResolver.Shared.GetGroupAndType(name)!.Value;

        if (encoding == TextEncoding.UTF8) {
            writer.Write<byte>(0xE);
            writer.Write((byte)group);
            writer.Write((byte)type);
            if (Resolver.WriteBinaryUtf8(writer, group, type, @params) == false) {
                DefaultTagResolver.Shared.WriteBinaryUtf8(writer, group, type, @params);
            }
        }
        else {
            writer.Write<ushort>(0xE);
            writer.Write(group);
            writer.Write(type);
            if (Resolver.WriteBinaryUtf16(writer, group, type, @params) == false) {
                DefaultTagResolver.Shared.WriteBinaryUtf16(writer, group, type, @params);
            }
        }
    }

    internal static void WriteEndTag(this StringBuilder sb, ushort group, ushort type)
    {
        sb.Append("<[");
        sb.Append(Resolver.GetName(group, type) ?? DefaultTagResolver.Shared.GetName(group, type));
        sb.Append("]>");
    }

    internal static void WriteEndTag(this RevrsWriter writer, ReadOnlySpan<char> text, TextEncoding encoding)
    {
        (ushort group, ushort type) = Resolver.GetGroupAndType(text[2..^2])
            ?? DefaultTagResolver.Shared.GetGroupAndType(text[2..^2])!.Value;

        if (encoding == TextEncoding.UTF8) {
            writer.Write<byte>(0xF);
            writer.Write((byte)group);
            writer.Write((byte)type);
        }
        else {
            writer.Write<ushort>(0xF);
            writer.Write(group);
            writer.Write(type);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ReadOnlySpan<char> ReadTagName(in this ReadOnlySpan<char> text, out int paramsStartIndex)
    {
        paramsStartIndex = text.IndexOf(' ');
        if (paramsStartIndex == -1) {
            paramsStartIndex = text.IndexOf('/');
        }

        return text[1..paramsStartIndex++];
    }
}
