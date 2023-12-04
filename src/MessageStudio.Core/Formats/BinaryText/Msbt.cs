using MessageStudio.Core.Common;

namespace MessageStudio.Core.Formats.BinaryText;

public struct Msbt
{
    public static Msbt FromBinary(in Memory<byte> buffer)
    {
        MemoryReader reader = new(buffer);
        return new(new(reader));
    }

    public Msbt()
    {

    }

    public Msbt(in MsbtReader reader)
    {

    }
}
