using MessageStudio.Core.Formats.BinaryText.Structures;
using MessageStudio.Core.Formats.BinaryText.Writers;
using MessageStudio.Core.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace MessageStudio.Core.Formats.BinaryText;

public enum Encoding : byte
{
    UTF8 = 0,
    Unicode = 1,
}

public class Msbt : Dictionary<string, MsbtEntry>
{
    internal const ulong MSBT_MAGIC = 0x6E4264745367734D;
    internal const uint ATR1_MAGIC = 0x31525441;
    internal const uint LBL1_MAGIC = 0x314C424C;
    internal const uint TXT2_MAGIC = 0x32545854;

    /// <summary>
    /// Create a new <see cref="Msbt"/> object from a data buffer
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static Msbt FromBinary(Span<byte> buffer)
    {
        SpanReader reader = new(buffer);
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
        Msbt managed = [];
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

    public void ToBinary(in Stream stream, Encoding encoding = Encoding.Unicode, Endian endianness = Endian.Little)
    {
        using InternalWriter writer = new(stream, endianness);
        ushort sectionCount = 0;
        bool isUsingATR1 = this.Any(x => !string.IsNullOrEmpty(x.Value.Attribute));

        writer.Seek(Unsafe.SizeOf<MsbtHeader>());

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
                    writer, encoding, sorted.Select(x => x.Value.Attribute).ToArray());
            });
        }

        MsbtSectionHeader.WriteSection(writer, ref sectionCount, TXT2_MAGIC, () => {
            TextSectionWriter.Write(writer, encoding, sorted.Values.Select(x => x.Text).ToArray());
        });

        MsbtHeader header = new(
            magic: MSBT_MAGIC,
            byteOrderMark: Endian.Big,
            encoding: encoding,
            version: 3,
            sectionCount: sectionCount,
            fileSize: (uint)writer.Position
        );

        writer.Seek(0);
        writer.Write<MsbtHeader, MsbtHeader.Reverser>(header);
    }

    /// <summary>
    /// Convert a MSBT file buffer to Yaml without allocating a <see cref="Msbt"/> object
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static string ToYaml(Span<byte> buffer)
    {
        SpanReader reader = new(buffer);
        ImmutableMsbt msbt = new(ref reader);
        StringBuilder sb = new();
        msbt.WriteYaml(sb);
        return sb.ToString();
    }
}
