using MessageStudio.Core.Common;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;

namespace MessageStudio.Core.Formats.BinaryText.Structures.Sections;

public unsafe readonly struct MsbtLabel
{
    private readonly int _valueLength;
    private readonly byte* _valuePtr;

    public readonly int Index;

    public string Value {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Utf8StringMarshaller.ConvertToManaged(_valuePtr)![.._valueLength];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> GetUtf8Bytes()
    {
        return new(_valuePtr, _valueLength);
    }

    public MsbtLabel(Memory<byte> buffer, Endian endian)
    {
        MemoryReader parser = new(buffer, endian);
        _valueLength = buffer.Length - 4;
        fixed (byte* ptr = buffer.Span[.._valueLength]) {
            _valuePtr = ptr;
        }

        parser.Move(_valueLength);
        Index = parser.Read<int>();
    }
}