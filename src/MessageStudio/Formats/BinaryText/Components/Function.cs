using CommunityToolkit.HighPerformance.Buffers;
using MessageStudio.Common;
using MessageStudio.Formats.BinaryText.Extensions;
using Revrs;
using System.Collections.Frozen;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using VYaml.Parser;

namespace MessageStudio.Formats.BinaryText.Components;

public class Function
{
    private readonly List<(string Type, string Name)> _fields;

    public string Name { get; }

    public Function(ref YamlParser parser)
    {
        if (!parser.TryGetCurrentTag(out Tag? rootTag)) {
            throw new InvalidOperationException("""
                    Function name was null.
                    """);
        }
        Name = rootTag.Suffix;

        _fields = [];
        parser.SkipAfter(ParseEventType.SequenceStart);
        while (parser.CurrentEventType is not ParseEventType.SequenceEnd) {
            if (!parser.TryGetCurrentTag(out Tag? tag)) {
                throw new InvalidOperationException("""
                    Invalid function field.
                    """);
            }

            _fields.Add((tag.Suffix, parser.ReadScalarAsString()
                ?? throw new InvalidOperationException("""
                    Function field name was null.
                    """)));
        }

        parser.SkipAfter(ParseEventType.SequenceEnd);
    }

    public void Append(StringBuilder sb, in Span<byte> data, in FrozenDictionary<string, FunctionEnum> enums, TextEncoding encoding)
    {
        RevrsReader reader = RevrsReader.Native(data);

        sb.Append(Name);
        
        foreach ((string type, string name) in _fields) {
            sb.Append(' ');
            sb.Append(name);
            sb.Append("='");

            switch (type) {
                case "u8" or "byte":
                    sb.Append(reader.Read<byte>());
                    break;
                case "bool" or "boolean":
                    sb.Append(reader.Read<bool>());
                    break;
                case "s16" or "short":
                    sb.Append(reader.Read<short>());
                    break;
                case "u16" or "ushort":
                    sb.Append(reader.Read<ushort>());
                    break;
                case "s32" or "int":
                    sb.Append(reader.Read<int>());
                    break;
                case "u32" or "uint":
                    sb.Append(reader.Read<uint>());
                    break;
                case "s64" or "long":
                    sb.Append(reader.Read<long>());
                    break;
                case "u64" or "ulong":
                    sb.Append(reader.Read<ulong>());
                    break;
                case "f16" or "half":
                    sb.Append(reader.Read<Half>());
                    break;
                case "f32" or "float":
                    sb.Append(reader.Read<float>());
                    break;
                case "f64" or "double":
                    sb.Append(reader.Read<double>());
                    break;
                case "str" or "string": {
                    ushort stringLength = reader.Read<ushort>();
                    switch (encoding) {
                        case TextEncoding.UTF8: {
                            Span<byte> utf8 = reader.ReadSpan<byte>(stringLength);
                            int size = Encoding.UTF8.GetCharCount(utf8);
                            using SpanOwner<char> managed = SpanOwner<char>.Allocate(size);
                            Encoding.UTF8.GetChars(utf8, managed.Span);
                            sb.Append(managed.Span);
                            break;
                        }
                        case TextEncoding.Unicode: {
                            Span<char> utf16 = reader.ReadSpan<char>(stringLength / 2);
                            sb.Append(utf16);
                            break;
                        }
                    }
                    break;
                }
                case "char*" or "cstr" or "cstring":
                    sb.Append(ReadCString(ref reader, encoding));
                    break;
                case "hex" or "hexidecimal":
                    sb.AppendHex(data);
                    break;
                default: {
                    if (enums.TryGetValue(type, out FunctionEnum? functionEnum)) {
                        sb.Append(functionEnum.GetEnumValueName(ref reader));
                        break;
                    }

                    throw new InvalidOperationException($"""
                        Unsupported function parameter type: '{type}'
                        """);
                }
            };

            sb.Append('\'');
        }
    }

    public void Write(ref RevrsWriter writer, ref FunctionParser parser, in FrozenDictionary<string, FunctionEnum> enums, TextEncoding encoding)
    {
        int size = 0;
        long sizeOffset = writer.Position;
        writer.Move(2);

        foreach ((string type, string name) in _fields) {
            switch (type) {
                case "u8" or "byte":
                    size += sizeof(byte);
                    writer.Write(parser.Get<byte>(name));
                    break;
                case "bool" or "boolean":
                    size += sizeof(bool);
                    writer.Write(parser.Get<bool>(name));
                    break;
                case "s16" or "short":
                    size += sizeof(short);
                    writer.Write(parser.Get<short>(name));
                    break;
                case "u16" or "ushort":
                    size += sizeof(short);
                    writer.Write(parser.Get<ushort>(name));
                    break;
                case "s32" or "int":
                    size += sizeof(int);
                    writer.Write(parser.Get<int>(name));
                    break;
                case "u32" or "uint":
                    size += sizeof(uint);
                    writer.Write(parser.Get<uint>(name));
                    break;
                case "s64" or "long":
                    size += sizeof(long);
                    writer.Write(parser.Get<long>(name));
                    break;
                case "u64" or "ulong":
                    size += sizeof(ulong);
                    writer.Write(parser.Get<ulong>(name));
                    break;
                case "f16" or "half":
                    size += 2;
                    writer.Write(parser.Get<Half>(name));
                    break;
                case "f32" or "float":
                    size += sizeof(float);
                    writer.Write(parser.Get<float>(name));
                    break;
                case "f64" or "double":
                    size += sizeof(double);
                    writer.Write(parser.Get<double>(name));
                    break;
                case "str" or "string": {
                    ReadOnlySpan<char> encodedString = parser[name];
                    switch (encoding) {
                        case TextEncoding.UTF8: {
                            int stringLength = Encoding.UTF8.GetByteCount(encodedString);
                            size += sizeof(ushort) + stringLength;
                            using SpanOwner<byte> utf8 = SpanOwner<byte>.Allocate(stringLength);
                            Encoding.UTF8.GetBytes(encodedString, utf8.Span);
                            writer.Write((ushort)stringLength);
                            writer.Write(utf8.Span);
                            break;
                        }
                        case TextEncoding.Unicode: {
                            ushort stringLength = (ushort)(encodedString.Length * 2);
                            size += sizeof(ushort) + stringLength;
                            writer.Write(stringLength);
                            writer.Write(MemoryMarshal.Cast<char, byte>(encodedString));
                            break;
                        }
                    }
                    break;
                }
                case "char*" or "cstr" or "cstring":
                    ReadOnlySpan<char> encodedCString = parser[name];
                    switch (encoding) {
                        case TextEncoding.UTF8: {
                            int stringLength = Encoding.UTF8.GetByteCount(encodedCString);
                            size += stringLength + 1;
                            using SpanOwner<byte> utf8 = SpanOwner<byte>.Allocate(stringLength);
                            Encoding.UTF8.GetBytes(encodedCString, utf8.Span);
                            writer.Write(utf8.Span);
                            writer.Write(0x0);
                            break;
                        }
                        case TextEncoding.Unicode: {
                            ReadOnlySpan<byte> unicode = MemoryMarshal.Cast<char, byte>(encodedCString);
                            size += unicode.Length + 1;
                            size += size.AlignUp(2);
                            writer.Write(unicode);
                            writer.Write(0x0);
                            writer.Align(2);
                            break;
                        }
                    }
                    break;
                case "hex" or "hexidecimal":
                    ReadOnlySpan<char> hex = parser[name][2..];
                    size += hex.Length / 2;
                    for (int i = 0; i < hex.Length; i++) {
                        writer.Write(byte.Parse(hex[i..(++i + 1)], NumberStyles.HexNumber));
                    }
                    break;
                default: {
                    if (enums.TryGetValue(type, out FunctionEnum? functionEnum)) {
                        string enumName = parser[name].ToString();
                        functionEnum.WriteEnumValue(ref writer, ref size, enumName);
                        break;
                    }

                    throw new InvalidOperationException($"""
                        Unsupported function parameter type: '{type}'
                        """);
                }
            };
        }

        long offset = writer.Position;

        writer.Seek(sizeOffset);
        writer.Write(encoding switch {
            TextEncoding.UTF8 => (byte)size,
            TextEncoding.Unicode => (ushort)size,
            _ => throw new NotSupportedException($"""
                The text encoding '{encoding}' is not supported.
                """)
        });

        writer.Seek(offset);
    }

    private unsafe string ReadCString(ref RevrsReader reader, TextEncoding encoding)
    {
        switch (encoding) {
            case TextEncoding.UTF8: {
                fixed (byte* ptr = &reader.Read<byte>()) {
                    return Utf8StringMarshaller.ConvertToManaged(ptr) ?? string.Empty;
                }
            }
            case TextEncoding.Unicode: {
                fixed (ushort* ptr = &reader.Read<ushort>()) {
                    return Utf16StringMarshaller.ConvertToManaged(ptr) ?? string.Empty;
                }
            }
        }

        throw new NotSupportedException($"""
            The text encoding '{encoding}' is not supported.
            """);
    }
}
