using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;

namespace MessageStudio.Core.Formats.BinaryText.Structures.Sections;

public unsafe class MsbtAttribute(int index, ushort* valuePtr)
{
    private readonly ushort* _valuePtr = valuePtr;
    private string? _value = null;

    public int Index { get; } = index;

    public string? Value {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => string.IsNullOrEmpty(
            _value ??= Utf16StringMarshaller.ConvertToManaged(_valuePtr)) ? _value = null : _value;
    }
}