using MessageStudio.Core.Common;
using System.Runtime.CompilerServices;

namespace MessageStudio.Core.Formats.BinaryText;

public class Msbt : Dictionary<string, MsbtEntry>
{
    public ReadOnlyMsbt ReadOnly { get; }

    public static Msbt FromBinary(in Memory<byte> buffer)
        => new(buffer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string? ToYaml()
    {
        StringBuilder sb = new();
        if (UsesAttributes()) {
            WriteWithAttributes(ref sb);
        }
        else {
            Write(ref sb);
        }

        return sb.ToString();
    }

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Write(ref StringBuilder sb)
    {
        foreach ((var label, var entry) in this) {
            sb.Append(label);
            sb.Append(": |-\n  ");
            sb.AppendLine(entry.Text.Replace("\n", "\n  "));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteWithAttributes(ref StringBuilder sb)
    {
        foreach ((var label, var entry) in this) {
            sb.Append(label);
            sb.AppendLine(":");
            sb.Append("  Attribute: ");
            sb.AppendLine(entry.Attribute ?? "~");
            sb.Append("  Text: |-\n    ");
            sb.AppendLine(entry.Text.Replace("\n", "\n    "));
        }
    }
}
