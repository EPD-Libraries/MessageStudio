using BenchmarkDotNet.Attributes;
using MessageStudio.Core.Common;
using MessageStudio.Core.Formats.BinaryText;
using MessageStudio.Core.Formats.BinaryText.Structures.Sections;
using MsbtLib;

namespace MessageStudio.Runner.Benchmarks;

[MemoryDiagnoser(true)]
public class MsbtParserBenchmarks
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
        Parser parser = new(_bufferLe);
        MsbtReader reader = new(ref parser);
        foreach (MsbtLabelSection.MsbtLabel label in reader.LabelSection) {
            _ = label.Index;
            _ = label.Value;
        }
        foreach (MsbtAttributeSection.MsbtAttribute atr in reader.AttributeSection) {
            _ = atr.Index;
            _ = atr.Value;
        }
        foreach (MsbtTextSection.MsbtText txt in reader.TextSection) {
            _ = txt.Index;
            _ = txt.Value;
        }
    }

    [Benchmark]
    public void ParseBE()
    {
        Parser parser = new(_bufferBe);
        MsbtReader reader = new(ref parser);
        foreach (MsbtLabelSection.MsbtLabel label in reader.LabelSection) {
            _ = label.Index;
            _ = label.Value;
        }
        foreach (MsbtAttributeSection.MsbtAttribute atr in reader.AttributeSection) {
            _ = atr.Index;
            _ = atr.Value;
        }
        foreach (MsbtTextSection.MsbtText txt in reader.TextSection) {
            _ = txt.Index;
            _ = txt.Value;
        }
    }

    [Benchmark]
    public void ParseBELarge()
    {
        Parser parser = new(_bufferBeLarge);
        MsbtReader reader = new(ref parser);
        foreach (MsbtLabelSection.MsbtLabel label in reader.LabelSection) {
            _ = label.Index;
            _ = label.Value;
        }
        foreach (MsbtAttributeSection.MsbtAttribute atr in reader.AttributeSection) {
            _ = atr.Index;
            _ = atr.Value;
        }
        foreach (MsbtTextSection.MsbtText txt in reader.TextSection) {
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
