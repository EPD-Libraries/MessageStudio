using System.Text;

namespace MessageStudio.Formats.BinaryText.Parsers;

public readonly ref struct YamlParser(ReadOnlySpan<char> yaml, Msbt target)
{
    internal const string ATTRIBUTE_PARAM_NAME = "Attribute";
    internal const string TEXT_PARAM_NAME = "Text";

    private  readonly Msbt _target = target;

    public readonly ReadOnlySpan<char> Yaml = yaml;

    public void Parse()
    {
        bool isReadingText = false;
        int attributeIndex = -1;

        int indentSize = 2;
        int keyStartPos = 0;

        ReadOnlySpan<char> key = [];
        ReadOnlySpan<char> attribute = [];
        StringBuilder text = new();

        for (int i = 0; i < Yaml.Length; i++) {
            char @char = Yaml[i];

            if (@char is '\r') {
                continue;
            }

            if (isReadingText) {
                if (@char is '\n') {
                    if (!TryLookNext(i, out char next) || next is not ' ') {
                        isReadingText = false;
                        keyStartPos = i + 1;
                        indentSize = 2;
                        continue;
                    }

                    i += indentSize;
                }

                text.Append(@char);
            }
            else if (attributeIndex > 0 && @char is '\n') {
                attribute = Yaml[attributeIndex..(i - 1)];
                attributeIndex = -1;
                keyStartPos = i + 1 + indentSize;
            }
            else if (@char is ':') {
                ReadOnlySpan<char> yamlKey = Yaml[keyStartPos..i];
                SkipWhitespace(ref i);
                
                if (yamlKey is ATTRIBUTE_PARAM_NAME) {
                    attributeIndex = i + 1;
                }
                else if (yamlKey is TEXT_PARAM_NAME) {
                    isReadingText = true;
                    indentSize += 2;
                    i += 3 + indentSize;
                }
                else {
                    if (!key.IsEmpty) {
                        AddEntry(key, attribute, text);
                    }

                    key = yamlKey;
                }
            }
            else if (@char is '|' && TryLookNext(i, out char next) && next is '-') {
                isReadingText = true;
                i++;
                SkipNewlines(ref i);
                i += indentSize;
            }
            else if (@char is '\n') {
                keyStartPos = (i += indentSize) + 1;
            }
        }

        AddEntry(key, attribute, text);
    }

    private bool TryLookNext(int index, out char @char)
    {
        if (Yaml.Length <= ++index) {
            @char = default;
            return false;
        }

        @char = Yaml[index];
        return true;
    }

    private void SkipWhitespace(ref int index)
    {
        while (TryLookNext(index, out char next) && next is ' ') {
            index++;
        }
    }

    private void SkipNewlines(ref int index)
    {
        while (TryLookNext(index, out char next) && next is '\r' or '\n') {
            index++;
        }
    }

    private void AddEntry(ReadOnlySpan<char> key, ReadOnlySpan<char> attribute, StringBuilder text)
    {
        _target[key.ToString()] = new() {
            Attribute = attribute.IsEmpty || attribute is "~" ? null : attribute.ToString(),
            Text = text.ToString()
        };

        text.Clear();
    }
}
