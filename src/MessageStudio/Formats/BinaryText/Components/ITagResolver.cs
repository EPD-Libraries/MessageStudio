using Revrs;
using System.Text;

namespace MessageStudio.Formats.BinaryText.Components;

public interface ITagResolver
{
    /// <summary>
    /// Get the tag name, or null if the <paramref name="group"/>/<paramref name="type"/> pair in unknown.
    /// </summary>
    /// <param name="group"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public string? GetName(ushort group, ushort type);

    /// <summary>
    /// Attempts to parse the <paramref name="name"/> into a group/type pair.
    /// </summary>
    /// <param name="group"></param>
    /// <param name="type"></param>
    /// <returns>
    /// <see langword="true"/> if the operation was successful, otherwise <see langword="false"/>.
    /// </returns>
    public (ushort, ushort)? GetGroupAndType(ReadOnlySpan<char> name);

    /// <inheritdoc cref="WriteText(StringBuilder, ReadOnlySpan{char}, Span{byte})"/>
    public bool WriteBinaryUtf16(RevrsWriter writer, ReadOnlySpan<char> name, in TagParams @params);

    /// <inheritdoc cref="WriteText(StringBuilder, ReadOnlySpan{char}, Span{byte})"/>
    public bool WriteBinaryUtf8(RevrsWriter writer, ReadOnlySpan<char> name, in TagParams @params);

    /// <summary>
    /// Returns <see langword="true"/> if the tag was recognized
    /// and written and <see langword="false"/> if the tag is unknown.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="params"></param>
    /// <returns></returns>
    public void WriteText(StringBuilder sb, ReadOnlySpan<char> name, Span<byte> data);
}
