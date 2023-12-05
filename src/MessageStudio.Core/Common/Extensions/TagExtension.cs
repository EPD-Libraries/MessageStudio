using System.Runtime.CompilerServices;

namespace MessageStudio.Core.Common.Extensions;

public static class TagExtension
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<char> ReadTagName(in this ReadOnlySpan<char> text)
    {
        return text[1..text.IndexOf(' ')];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<char> ReadProperty(in this ReadOnlySpan<char> text, in ReadOnlySpan<char> name)
    {
        int startIndex = text.IndexOf(name) + name.Length + 2;
        int endIndex = startIndex + text[startIndex..].IndexOf('\'');
        return text[startIndex..endIndex];
    }
}
