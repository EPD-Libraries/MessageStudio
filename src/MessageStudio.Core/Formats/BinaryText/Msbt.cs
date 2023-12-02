using MessageStudio.Core.Common;

namespace MessageStudio.Core.Formats.BinaryText;

public struct Msbt(in MsbtReader reader)
{
    public static Msbt FromBinary(in Span<byte> buffer)
    {
        Parser parser = new(buffer);
        return new(new(ref parser));
    }
}
