namespace MessageStudio.Core.Formats.BinaryText;

public class MsbtEntry
{
    /// <summary>
    /// The entry attribute from the attribute section (Optional)
    /// </summary>
    public string? Attribute { get; set; }

    /// <summary>
    /// The pseudo-HTML processed text
    /// </summary>
    public required string Text { get; set; }
}
