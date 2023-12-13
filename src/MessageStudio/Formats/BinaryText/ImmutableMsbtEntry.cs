using MessageStudio.Formats.BinaryText.Structures;

namespace MessageStudio.Formats.BinaryText;

public ref struct ImmutableMsbtEntry(MsbtLabel label, MsbtAttribute attribute, MsbtText text)
{
    public MsbtLabel Label = label;
    public MsbtAttribute Attribute = attribute;
    public MsbtText Text = text;
}
