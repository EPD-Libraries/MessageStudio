using MessageStudio.Common;
using MessageStudio.Formats.BinaryText.Extensions;
using Revrs;
using Revrs.Buffers;
using System.Buffers;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using VYaml.Parser;

namespace MessageStudio.Formats.BinaryText.Components;

public class FunctionMap
{
    public static readonly FunctionMap Default = new();

    public static FunctionMap Current { get; set; } = Default;

    private readonly FrozenDictionary<string, FunctionEnum> _enums;
    private readonly FrozenDictionary<(int, int), Function> _functions;
    private readonly FrozenDictionary<string, (int, int)> _functionNames;

    public FunctionMap()
    {
        _enums = new Dictionary<string, FunctionEnum>().ToFrozenDictionary();
        _functions = new Dictionary<(int, int), Function>().ToFrozenDictionary();
        _functionNames = new Dictionary<string, (int, int)>().ToFrozenDictionary();
    }

    public FunctionMap(ref YamlParser parser)
    {
        parser.SkipAfter(ParseEventType.MappingStart);

        Dictionary<string, FunctionEnum> enums = [];
        while (parser.CurrentEventType is not ParseEventType.MappingEnd) {
            string key = parser.ReadScalarAsString()
                ?? throw new InvalidOperationException("""
                    Function enum name was null.
                    """);

            enums.Add(key, new FunctionEnum(ref parser));
        }

        Dictionary<(int, int), Function> functions = [];
        Dictionary<string, (int, int)> functionNames = [];

        parser.SkipAfter(ParseEventType.MappingStart);
        while (parser.CurrentEventType is not ParseEventType.MappingEnd) {
            parser.SkipAfter(ParseEventType.SequenceStart);
            int group = parser.ReadScalarAsInt32();
            int type = parser.ReadScalarAsInt32();
            parser.SkipAfter(ParseEventType.SequenceEnd);

            Function function = new(ref parser);
            functions.Add((group, type), function);

            functionNames.Add(function.Name, (group, type));
        }

        _enums = enums.ToFrozenDictionary();
        _functions = functions.ToFrozenDictionary();
        _functionNames = functionNames.ToFrozenDictionary();
    }

    public static FunctionMap FromFile(string filePath)
    {
        using FileStream fs = File.OpenRead(filePath);
        int size = Convert.ToInt32(fs.Length);
        using ArraySegmentOwner<byte> buffer = ArraySegmentOwner<byte>.Allocate(size);
        fs.Read(buffer.Segment);
        return FromBinary(buffer.Segment);
    }

    public static FunctionMap FromBinary(ArraySegment<byte> data)
    {
        ReadOnlySequence<byte> sequence = new(data);
        YamlParser parser = new(sequence);
        return new(ref parser);
    }

    public void AppendFunction(StringBuilder sb, int group, int type, in Span<byte> data, TextEncoding encoding)
    {
        sb.Append('<');

        if (_functions.TryGetValue((group, type), out Function? function)) {
            function.Append(sb, data, _enums, encoding);
        }
        else {
            sb.Append(group);
            sb.Append('|');
            sb.Append(type);
            sb.Append(" Data='");
            sb.AppendHex(data);
            sb.Append('\'');
        }

        sb.Append("/>");
    }

    public void AppendEmptyFunction(StringBuilder sb, int group, int type)
    {
        sb.Append("<[");

        if (_functions.TryGetValue((group, type), out Function? function)) {
            sb.Append(function.Name);
            return;
        }
        else {
            sb.Append(group);
            sb.Append('|');
            sb.Append(type);
        }

        sb.Append("]>");
    }

    public void WriteFunctionParams(ref RevrsWriter writer, in ReadOnlySpan<char> text, TextEncoding encoding)
    {
        if (text.Length < 4) {
            throw new InvalidDataException($"""
                Invalid function: '{text}'
                """);
        }

        string functionName = GetFunctionName(text, out int paramsStartIndex, out byte leadingByte)
            .ToString();

        if (!TryGetFunctionId(functionName, out (int group, int type) id)) {
            int split = functionName.IndexOf('|');
            id = (int.Parse(functionName[..split]), int.Parse(functionName[++split..]));
        }

        (int group, int type) = id;
        FunctionParser functionParser = new(text[paramsStartIndex..]);

        switch (encoding) {
            case TextEncoding.UTF8:
                writer.Write(leadingByte);
                writer.Write((byte)group);
                writer.Write((byte)type);
                break;
            case TextEncoding.Unicode:
                writer.Write<ushort>(leadingByte);
                writer.Write((ushort)group);
                writer.Write((ushort)type);
                break;
        }

        // Empty function type
        if (leadingByte is 0xF) {
            return;
        }

        if (_functions.TryGetValue((group, type), out Function? function)) {
            function.Write(ref writer, ref functionParser, _enums, encoding);
            return;
        }

        ReadOnlySpan<char> data = functionParser["Data"][2..];
        int size = data.Length / 2;
        writer.Write(encoding switch {
            TextEncoding.UTF8 => (byte)size,
            TextEncoding.Unicode => (ushort)size,
            _ => throw new NotSupportedException($"""
                The text encoding '{encoding}' is not supported.
                """)
        });

        for (int i = 0; i < data.Length; i++) {
            writer.Write(byte.Parse(data[i..(++i + 1)], NumberStyles.HexNumber));
        }

        if (data.Length > 0) {
            writer.Align(encoding switch {
                TextEncoding.UTF8 => 1,
                TextEncoding.Unicode => 2,
                _ => throw new NotSupportedException($"""
                    The text encoding '{encoding}' is not supported.
                    """)
            });
        }
    }

    public bool TryGetFunctionId(string name, [MaybeNullWhen(false)] out (int, int) id)
    {
        return _functionNames.TryGetValue(name, out id);
    }

    private static ReadOnlySpan<char> GetFunctionName(in ReadOnlySpan<char> text, out int paramsStartIndex, out byte leadingByte)
    {
        if (text[1] == '[') {
            leadingByte = 0xF;
            paramsStartIndex = text.IndexOf(']');
            if (paramsStartIndex == -1) {
                throw new InvalidDataException($"""
                    Invalid function: '{text}'
                    """);
            }

            return text[1..paramsStartIndex++];
        }
        else {
            leadingByte = 0xE;
            paramsStartIndex = text.IndexOf(' ');
            if (paramsStartIndex == -1) {
                paramsStartIndex = text.IndexOf('/');
            }

            return text[1..paramsStartIndex++];
        }
    }
}
