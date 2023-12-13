using BenchmarkDotNet.Attributes;
using MessageStudio.Formats.BinaryText;
using MsbtLib;

namespace MessageStudio.Runner.Benchmarks;

[MemoryDiagnoser(true)]
public class MsbtWriteBenchmarks
{
    private Msbt _newMsbtLe = null!;
    private readonly MemoryStream _newMsbtLeMs = new();

    private Msbt _newMsbtBe = null!;
    private readonly MemoryStream _newMsbtBeMs = new();

    private Msbt _newMsbtBeLarge = null!;
    private readonly MemoryStream _newMsbtBeLargeMs = new();

    private MSBT _msbtLe = null!;
    private readonly MemoryStream _msbtLeMs = new();

    private MSBT _msbtBe = null!;
    private readonly MemoryStream _msbtBeMs = new();

    private MSBT _msbtBeLarge = null!;
    private readonly MemoryStream _msbtBeLargeMs = new();


    [GlobalSetup]
    public void Setup()
    {
        _newMsbtLe = Msbt.FromBinary(File.ReadAllBytes("D:\\bin\\Msbt\\100enemy-LE.msbt"));
        _newMsbtBe = Msbt.FromBinary(File.ReadAllBytes("D:\\bin\\Msbt\\100enemy.msbt"));
        _newMsbtBeLarge = Msbt.FromBinary(File.ReadAllBytes("D:\\bin\\Msbt\\ArmorHead.msbt"));

        _msbtLe = new MSBT(File.ReadAllBytes("D:\\bin\\Msbt\\100enemy-LE.msbt"));
        _msbtBe = new MSBT(File.ReadAllBytes("D:\\bin\\Msbt\\100enemy.msbt"));
        _msbtBeLarge = new MSBT(File.ReadAllBytes("D:\\bin\\Msbt\\ArmorHead.msbt"));
    }

    [Benchmark]
    public void WriteLE()
    {
        _newMsbtLe.ToBinary(_newMsbtLeMs);
    }

    [Benchmark]
    public void WriteBE()
    {
        _newMsbtBe.ToBinary(_newMsbtBeMs);
    }

    [Benchmark]
    public void WriteBELarge()
    {
        _newMsbtBeLarge.ToBinary(_newMsbtBeLargeMs);
    }

    [Benchmark]
    public void WriteLE_MsbtLib()
    {
        _msbtLeMs.Write(_msbtLe.Write());
    }

    [Benchmark]
    public void WriteBE_MsbtLib()
    {
        _msbtBeMs.Write(_msbtBe.Write());
    }

    [Benchmark]
    public void WriteBELarge_MsbtLib()
    {
        _msbtBeLargeMs.Write(_msbtBeLarge.Write());
    }
}
