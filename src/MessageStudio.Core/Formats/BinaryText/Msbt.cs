using MessageStudio.Core.Common;
using System.Runtime.CompilerServices;

namespace MessageStudio.Core.Formats.BinaryText;

public class Msbt : Dictionary<string, MsbtEntry>
{
    public ReadOnlyMsbt ReadOnly { get; }

    public static Msbt FromBinary(in Memory<byte> buffer)
        => new(buffer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string? ToYaml() => ReadOnly.ToYaml();

    public Msbt()
    {
    }

    public Msbt(in Memory<byte> buffer) : this(new ReadOnlyMsbt(new MemoryReader(buffer)))
    {
    }

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
