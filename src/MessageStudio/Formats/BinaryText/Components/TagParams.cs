namespace MessageStudio.Formats.BinaryText.Components;

public readonly ref struct TagParams(ReadOnlySpan<char> text)
{
    private readonly ReadOnlySpan<char> _text = text;

	public ReadOnlySpan<char> this[ReadOnlySpan<char> name] {
		get {
			int index = _text.IndexOf(name);
			if (index == -1) {
				return [];
			}

			index += name.Length + 2;
			int endIndex = _text[index..].IndexOf('\'');
			if (endIndex == -1) {
				throw new InvalidDataException($"""
					Invalid parameter '{name}': a closing quote could not be found in {_text}
					""");
			}

			return _text[index..(index + endIndex)];
		}
	}

	public T GetEnum<T>(ReadOnlySpan<char> name) where T : struct
	{
		return Enum.Parse<T>(this[name]);
	}

	public T Get<T>(ReadOnlySpan<char> name) where T : ISpanParsable<T>
	{
		return T.Parse(this[name], null);
	}

	public bool TryGet<T>(ReadOnlySpan<char> name, out T? value) where T : ISpanParsable<T>
	{
		return T.TryParse(this[name], null, out value);
	}
}
