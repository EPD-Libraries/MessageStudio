using Revrs;
using System.Runtime.InteropServices;

namespace MessageStudio.Formats.BinaryText.Structures;

[StructLayout(LayoutKind.Explicit, Pack = 4, Size = 8)]
public struct MsbtLabelGroup
{
    /// <summary>
    /// The number of labels in the group
    /// </summary>
    [FieldOffset(0x0)]
    public int LabelCount;

    /// <summary>
    /// Offset to the first label in the section
    /// relative to the beginning of the section
    /// </summary>
    [FieldOffset(0x4)]
    public int LabelOffset;

    public class Reverser : IStructReverser
    {
        public static void Reverse(in Span<byte> buffer)
        {
            buffer[0x0..0x4].Reverse();
            buffer[0x4..0x8].Reverse();
        }
    }
}