using MessageStudio.Common;
using MessageStudio.Formats.BinaryText.Parsers;
using MessageStudio.Formats.BinaryText.Structures;
using MessageStudio.Formats.BinaryText.Writers;
using Revrs;
using System.Text;

namespace MessageStudio.Formats.BinaryText;

public class Msbt : Dictionary<string, MsbtEntry>
{
    internal const ulong MSBT_MAGIC = 0x6E4264745367734D;
    internal const uint ATR1_MAGIC = 0x31525441;
    internal const uint LBL1_MAGIC = 0x314C424C;
    internal const uint TXT2_MAGIC = 0x32545854;

    public Endianness Endianness { get; set; } = Endianness.Little;
    public TextEncoding Encoding { get; set; } = TextEncoding.Unicode;

    /// <summary>
    /// Create a new <see cref="Msbt"/> object from a data buffer
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static Msbt FromBinary(Span<byte> buffer)
    {
        RevrsReader reader = new(buffer);
        ImmutableMsbt msbt = new(ref reader);
        return FromImmutable(ref msbt);
    }

    /// <summary>
    /// Create a new <see cref="Msbt"/> object from an <see cref="ImmutableMsbt"/>
    /// </summary>
    /// <param name="msbt"></param>
    /// <returns></returns>
    public static Msbt FromImmutable(ref ImmutableMsbt msbt)
    {
        Msbt managed = new() {
            Encoding = msbt.Header.Encoding,
            Endianness = msbt.Header.ByteOrderMark
        };

        foreach (var label in msbt.LabelSectionReader) {
            int index = label.Index;
            string? key = label.GetManaged();
            if (key is not null) {
                managed.Add(key, new MsbtEntry {
                    Attribute = msbt.AttributeSectionReader[index].GetManaged(),
                    Text = msbt.TextSectionReader[index].GetManaged()
                });
            }
        }

        return managed;
    }

    /// <summary>
    /// Create a new <see cref="Msbt"/> object from yaml text
    /// </summary>
    /// <param name="src"></param>
    /// <returns></returns>
    public static Msbt FromYaml(in ReadOnlySpan<char> src)
    {
        Msbt result = [];
        YamlParser parser = new(src, result);
        parser.Parse();

        return result;
    }

    public unsafe void ToBinary(in Stream stream, TextEncoding? encoding = null, Endianness? endianness = null)
    {
        endianness ??= Endianness;
        encoding ??= Encoding;

        RevrsWriter writer = new(stream, endianness.Value);
        ushort sectionCount = 0;
        bool isUsingATR1 = this.Any(x => !string.IsNullOrEmpty(x.Value.Attribute));

        writer.Seek(sizeof(MsbtHeader));

        // Sort by the attributes so that every
        // null/empty attribute is at the end
        Dictionary<string, MsbtEntry> sorted = isUsingATR1
            ? this
                .OrderBy(x => x.Value.Attribute)
                .OrderBy(x => string.IsNullOrEmpty(x.Value.Attribute))
                .ToDictionary(x => x.Key, x => x.Value)
            : this;

        MsbtSectionHeader.WriteSection(writer, ref sectionCount, LBL1_MAGIC, () => {
            LabelSectionWriter.Write(writer, sorted.Keys);
        });

        if (isUsingATR1) {
            MsbtSectionHeader.WriteSection(writer, ref sectionCount, ATR1_MAGIC, () => {
                AttributeSectionWriter.Write(
                    writer, encoding.Value, sorted.Select(x => x.Value.Attribute).ToArray());
            });
        }

        MsbtSectionHeader.WriteSection(writer, ref sectionCount, TXT2_MAGIC, () => {
            TextSectionWriter.Write(writer, encoding.Value, sorted.Values.Select(x => x.Text).ToArray());
        });

        MsbtHeader header = new(
            magic: MSBT_MAGIC,
            byteOrderMark: Endianness.Big,
            encoding: encoding.Value,
            version: 3,
            sectionCount: sectionCount,
            fileSize: (uint)writer.Position
        );

        writer.Seek(0);
        writer.Write<MsbtHeader, MsbtHeader.Reverser>(header);
    }

    public string ToYaml()
    {
        StringBuilder sb = new();

        if (this.Any(x => !string.IsNullOrEmpty(x.Value.Attribute))) {
            WriteYamlWithParameters(sb);
        }
        else {
            WriteYaml(sb);
        }

        return sb.ToString();
    }

    private void WriteYaml(in StringBuilder sb)
    {
        foreach ((var label, var entry) in this) {
            sb.Append(label);
            sb.Append(": |-\n  ");
            sb.AppendLine(entry.Text?.Replace("\n", "\n  "));
        }
    }

    private void WriteYamlWithParameters(in StringBuilder sb)
    {
        foreach ((var label, var entry) in this) {
            sb.Append(label);
            sb.Append($":\n  {YamlParser.ATTRIBUTE_PARAM_NAME}: ");
            sb.AppendLine(string.IsNullOrEmpty(entry.Attribute) ? "~" : entry.Attribute);
            sb.Append($"  {YamlParser.TEXT_PARAM_NAME}: |-\n    ");
            sb.AppendLine(entry.Text?.Replace("\n", "\n    "));
        }
    }
}
