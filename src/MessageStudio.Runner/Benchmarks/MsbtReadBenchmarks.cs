using BenchmarkDotNet.Attributes;
using MessageStudio.Core.Formats.BinaryText;
using MsbtLib;

namespace MessageStudio.Runner.Benchmarks;

[MemoryDiagnoser(true)]
public class MsbtReadBenchmarks
{
    private byte[] _bufferLe = [];
    private byte[] _bufferBe = [];
    private byte[] _bufferBeLarge = [];

    [GlobalSetup]
    public void Setup()
    {
        _bufferLe = File.ReadAllBytes("D:\\bin\\Msbt\\100enemy-LE.msbt");
        _bufferBe = File.ReadAllBytes("D:\\bin\\Msbt\\100enemy.msbt");
        _bufferBeLarge = File.ReadAllBytes("D:\\bin\\Msbt\\ArmorHead.msbt");
    }

    [Benchmark]
    public void ParseLE()
    {
        _ = Msbt.FromBinary(_bufferLe);
    }

    [Benchmark]
    public void ParseBE()
    {
        _ = Msbt.FromBinary(_bufferBe);
    }
    
    [Benchmark]
    public void ParseBELarge()
    {
        _ = Msbt.FromBinary(_bufferBeLarge);
    }
    
    [Benchmark]
    public void ParseLE_MsbtLib()
    {
        MSBT msbt = new(_bufferLe);
        msbt.GetTexts();
    }
    
    [Benchmark]
    public void ParseBE_MsbtLib()
    {
        MSBT msbt = new(_bufferBe);
        _ = msbt.GetTexts();
    }
    
    [Benchmark]
    public void ParseBELarge_MsbtLib()
    {
        MSBT msbt = new(_bufferBeLarge);
        _ = msbt.GetTexts();
    }
}
