using Revrs;
using System.Collections.Frozen;
using VYaml.Parser;

namespace MessageStudio.Formats.BinaryText.Components;

public class FunctionEnum
{
    private readonly string _type;
    private readonly FrozenDictionary<long, string> _map;
    private readonly FrozenDictionary<string, long> _values;

    public FunctionEnum(ref YamlParser parser)
    {
        if (!parser.TryGetCurrentTag(out Tag? tag) || !FunctionEnum.IsValidEnumType(tag.Suffix)) {
            throw new InvalidOperationException($"""
                    Invalid function enum type: '{tag?.Suffix ?? "null"}'
                    """);
        }

        _type = tag.Suffix;
        Dictionary<long, string> map = [];
        Dictionary<string, long> values = [];

        parser.SkipAfter(ParseEventType.MappingStart);
        while (parser.CurrentEventType is not ParseEventType.MappingEnd) {
            long value = parser.ReadScalarAsInt64();
            string name = parser.ReadScalarAsString() ?? throw new InvalidOperationException(
                """
                Function enum value cannot be null.
                """);

            map.Add(value, name);
            values.Add(name, value);
        }

        parser.SkipAfter(ParseEventType.MappingEnd);
        _map = map.ToFrozenDictionary();
        _values = values.ToFrozenDictionary();
    }

    public static bool IsValidEnumType(in ReadOnlySpan<char> type)
    {
        return type is
            "u8" or "byte" or
            "s16" or "short" or
            "u16" or "ushort" or
            "s32" or "int" or
            "u32" or "uint" or
            "s64" or "long";
    }

    public string GetEnumValueName(ref RevrsReader reader)
    {
        long key = _type switch {
            "u8" or "byte" => reader.Read<byte>(),
            "s16" or "short" => reader.Read<short>(),
            "u16" or "ushort" => reader.Read<ushort>(),
            "s32" or "int" => reader.Read<int>(),
            "u32" or "uint" => reader.Read<uint>(),
            "s64" or "long" => reader.Read<long>(),
            _ => throw new InvalidOperationException($"""
                Invalid Enum Type: '{_type}'
                """)
        };

        return _map.TryGetValue(key, out string? functionEnumName) switch {
            true => functionEnumName,
            false => key.ToString()
        };
    }

    public void WriteEnumValue(ref RevrsWriter writer, ref int size, string enumValueName)
    {
        long value = _values.TryGetValue(enumValueName, out long _value) switch {
            true => _value,
            false => long.Parse(enumValueName)
        };

        switch (_type) {
            case "u8" or "byte":
                size += sizeof(byte);
                writer.Write((byte)value);
                break;
            case "s16" or "short":
                size += sizeof(short);
                writer.Write((short)value);
                break;
            case "u16" or "ushort":
                size += sizeof(ushort);
                writer.Write((ushort)value);
                break;
            case "s32" or "int":
                size += sizeof(int);
                writer.Write((int)value);
                break;
            case "u32" or "uint":
                size += sizeof(uint);
                writer.Write((uint)value);
                break;
            case "s64" or "long":
                size += sizeof(long);
                writer.Write(value);
                break;
            default:
                throw new InvalidOperationException($"""
                    Invalid Enum Type: '{_type}'
                    """);
        };
    }
}
