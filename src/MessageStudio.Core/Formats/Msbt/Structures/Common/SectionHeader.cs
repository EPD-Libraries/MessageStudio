using MessageStudio.Core.Common;
using System.Runtime.InteropServices;

namespace MessageStudio.Core.Formats.Msbt.Structures.Common;

[StructLayout(LayoutKind.Sequential, Size = 12)]
public readonly struct SectionHeader
{
    public readonly int Size;
}
