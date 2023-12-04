using MessageStudio.Core.Common;

namespace MessageStudio.Core.Formats.BinaryText;

public class Msbt : Dictionary<string, MsbtEntry>
{
    public ReadOnlyMsbt ReadOnly { get; }

    public static Msbt FromBinary(in Memory<byte> buffer)
    {
        MemoryReader reader = new(buffer);
        return new(new(reader));
    }

    public Msbt() { }

    public Msbt(in ReadOnlyMsbt reader)
    {
        ReadOnly = reader;

        foreach (var label in reader.LabelSection) {
            Add(label.Value, new MsbtEntry {
                Attribute = reader.AttributeSection?[label.Index].Value,
                Text = reader.TextSection[label.Index].Value
            });
        }
    }
}
