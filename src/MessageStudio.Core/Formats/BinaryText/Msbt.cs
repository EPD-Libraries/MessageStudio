using MessageStudio.Core.Common;
using MessageStudio.Core.Formats.BinaryText.Structures;
using MessageStudio.Core.Formats.BinaryText.Structures.Sections.Writers;
using System.Runtime.CompilerServices;
using StringBuilder = System.Text.StringBuilder;

namespace MessageStudio.Core.Formats.BinaryText;

public class Msbt : Dictionary<string, MsbtEntry>
{
    public ReadOnlyMsbt ReadOnly { get; }

    public static Msbt FromBinary(in Memory<byte> buffer)
        => new(buffer);

    public void ToBinary(in Stream stream, Endian endianness = Endian.Little, Encoding encoding = Encoding.Unicode)
    {
        MemoryWriter writer = new(stream, endianness);
        ushort sectionCount = 0;
        bool usesAttributes = UsesAttributes();

        writer.Seek(MsbtHeader.Size);

        Dictionary<string, MsbtEntry> sorted = usesAttributes
            ? this
                .OrderBy(x => x.Value.Attribute)
                .OrderBy(x => x.Value.Attribute is null)
                .ToDictionary(x => x.Key, x => x.Value)
            : this;

        sectionCount++;
        MsbtLabelSectionWriter.Write(ref writer, sorted.Keys);

        if (UsesAttributes()) {
            sectionCount++;
            string?[] attributes = sorted.Select(x => x.Value.Attribute).ToArray();
            MsbtAttributeSectionWriter.Write(ref writer, encoding, attributes);
        }

        sectionCount++;
        MsbtTextSectionWriter.Write(ref writer, encoding, sorted.Values.Select(x => x.Text).ToArray());

        MsbtHeader header = new() {
            Encoding = Encoding.Unicode,
            Version = 3,
            SectionCount = sectionCount,
            FileSize = (uint)writer.Position,
        };

        writer.Seek(0);
        header.Write(ref writer);
    }

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
                Attribute = reader.AttributeSection?[label.Index]?.Value,
                Text = reader.TextSection[label.Index].Value
            });
        }
    }

    private bool UsesAttributes()
        => this.Any(x => !string.IsNullOrEmpty(x.Value.Attribute));

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
