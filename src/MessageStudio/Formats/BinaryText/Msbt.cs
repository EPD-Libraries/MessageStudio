using MessageStudio.Common;
using MessageStudio.Formats.BinaryText.Exceptions;
using MessageStudio.Formats.BinaryText.Parsers;
using MessageStudio.Formats.BinaryText.Structures;
using MessageStudio.Formats.BinaryText.Writers;
using Revrs;
using System.Text;

namespace MessageStudio.Formats.BinaryText;

public class Msbt : Dictionary<string, MsbtEntry>
{
    public const ulong MAGIC = 0x6E4264745367734D;

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
    public static Msbt FromBinary(Span<byte> buffer, MsbtOptions? options = null)
    {
        RevrsReader reader = new(buffer);
        ImmutableMsbt msbt = new(ref reader);
        return FromImmutable(ref msbt, options);
    }

    /// <summary>
    /// Create a new <see cref="Msbt"/> object from an <see cref="ImmutableMsbt"/>
    /// </summary>
    /// <param name="msbt"></param>
    /// <returns></returns>
    public static Msbt FromImmutable(ref ImmutableMsbt msbt, MsbtOptions? options = null)
    {
        options ??= MsbtOptions.Default;

        Msbt managed = new() {
            Encoding = msbt.Header.Encoding,
            Endianness = msbt.Header.ByteOrderMark
        };

        foreach (var label in msbt.LabelSectionReader) {
            int index = label.Index;
            string? key = label.GetManaged();
            if (key is not null) {
                switch (options.DuplicateKeyMode) {
                    case MsbtDuplicateKeyMode.UseLastOccurrence: {
                        managed[key] = new MsbtEntry {
                            Attribute = msbt.AttributeSectionReader[index].GetManaged(),
                            Text = msbt.TextSectionReader[index].GetManaged()
                        };

                        break;
                    }
                    default: {
                        bool keySuccessfullyAdded = managed.TryAdd(key, new MsbtEntry {
                            Attribute = msbt.AttributeSectionReader[index].GetManaged(),
                            Text = msbt.TextSectionReader[index].GetManaged()
                        });

                        if (!keySuccessfullyAdded && options.DuplicateKeyMode is MsbtDuplicateKeyMode.ThrowException) {
                            throw new MsbtDuplicateKeyException(key);
                        }

                        break;
                    }
                }
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

    /// <summary>
    /// <b>Note:</b> This method creates a copy of the written bytes.<br/>
    /// Use <see cref="WriteBinary(in Stream, TextEncoding?, Endianness?)"/> if writing to a stream is possible.
    /// </summary>
    /// <param name="encoding"></param>
    /// <param name="endianness"></param>
    /// <returns></returns>
    public byte[] ToBinary(TextEncoding? encoding = null, Endianness? endianness = null)
    {
        using MemoryStream ms = new();
        WriteBinary(ms, encoding, endianness);
        return ms.ToArray();
    }

    public unsafe void WriteBinary(in Stream stream, TextEncoding? encoding = null, Endianness? endianness = null)
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

        MsbtSectionHeader.WriteSection(ref writer, ref sectionCount, LBL1_MAGIC, () => {
            LabelSectionWriter.Write(ref writer, sorted.Keys);
        });

        if (isUsingATR1) {
            MsbtSectionHeader.WriteSection(ref writer, ref sectionCount, ATR1_MAGIC, () => {
                AttributeSectionWriter.Write(
                    ref writer, encoding.Value, sorted.Select(x => x.Value.Attribute).ToArray());
            });
        }

        MsbtSectionHeader.WriteSection(ref writer, ref sectionCount, TXT2_MAGIC, () => {
            TextSectionWriter.Write(ref writer, encoding.Value, sorted.Values.Select(x => x.Text).ToArray());
        });

        stream.SetLength(writer.Position);

        MsbtHeader header = new(
            magic: MAGIC,
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
