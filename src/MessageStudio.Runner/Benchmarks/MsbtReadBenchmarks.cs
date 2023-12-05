using BenchmarkDotNet.Attributes;
using MessageStudio.Core.Common;
using MessageStudio.Core.Formats.BinaryText;
using MessageStudio.Core.Formats.BinaryText.Structures.Sections;
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
        MemoryReader reader = new(_bufferLe);
        ReadOnlyMsbt msbt = new(reader);
        foreach (MsbtLabel label in msbt.LabelSection) {
            _ = label.Index;
            _ = label.Value;
        }
        foreach (MsbtAttribute atr in msbt.AttributeSection!) {
            _ = atr.Index;
            _ = atr.Value;
        }
        foreach (MsbtText txt in msbt.TextSection) {
            _ = txt.Index;
            _ = txt.Value;
        }
    }

    [Benchmark]
    public void ParseBE()
    {
        MemoryReader reader = new(_bufferBe);
        ReadOnlyMsbt msbt = new(reader);
        foreach (MsbtLabel label in msbt.LabelSection) {
            _ = label.Index;
            _ = label.Value;
        }
        foreach (MsbtAttribute atr in msbt.AttributeSection!) {
            _ = atr.Index;
            _ = atr.Value;
        }
        foreach (MsbtText txt in msbt.TextSection) {
            _ = txt.Index;
            _ = txt.Value;
        }
    }
    
    [Benchmark]
    public void ParseBELarge()
    {
        MemoryReader reader = new(_bufferBeLarge);
        ReadOnlyMsbt msbt = new(reader);
        foreach (MsbtLabel label in msbt.LabelSection) {
            _ = label.Index;
            _ = label.Value;
        }
        foreach (MsbtAttribute atr in msbt.AttributeSection!) {
            _ = atr.Index;
            _ = atr.Value;
        }
        foreach (MsbtText txt in msbt.TextSection) {
            _ = txt.Index;
            _ = txt.Value;
        }
    }
    
    [Benchmark]
    public void ParseLE_MsbtLib()
    {
        MSBT msbt = new(_bufferLe);
        foreach ((var _, var _) in msbt.GetTexts()) { }
    }
    
    [Benchmark]
    public void ParseBE_MsbtLib()
    {
        MSBT msbt = new(_bufferBe);
        foreach ((var _, var _) in msbt.GetTexts()) { }
    }
    
    [Benchmark]
    public void ParseBELarge_MsbtLib()
    {
        MSBT msbt = new(_bufferBeLarge);
        foreach ((var _, var _) in msbt.GetTexts()) { }
    }
}
