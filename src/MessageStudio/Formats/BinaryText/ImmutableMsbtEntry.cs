using MessageStudio.Formats.BinaryText.Structures;

namespace MessageStudio.Formats.BinaryText;

public ref struct ImmutableMsbtEntry(MsbtLabel label, MsbtAttribute attribute, MsbtText text)
{
    public MsbtLabel Label = label;
    public MsbtAttribute Attribute = attribute;
    public MsbtText Text = text;

    public readonly void Deconstruct(out MsbtLabel label, out MsbtText text, out MsbtAttribute attribute)
    {
        label = Label;
        text = Text;
        attribute = Attribute;
    }
}
