using BenchmarkDotNet.Attributes;
using MessageStudio.Core.Common;
using MessageStudio.Core.Formats.BinaryText;
using MessageStudio.Core.Formats.BinaryText.Structures.Sections;

namespace MessageStudio.Runner.Benchmarks;

[MemoryDiagnoser(true)]
public class MsbtParserBenchmarks
{
    private byte[] _bufferLe = [];
    private byte[] _bufferBe = [];

    [GlobalSetup]
    public void Setup()
    {
        _bufferLe = File.ReadAllBytes("D:\\bin\\Msbt\\100enemy-LE.msbt");
        _bufferBe = File.ReadAllBytes("D:\\bin\\Msbt\\100enemy.msbt");
    }

    [Benchmark]
    public void ParseLE()
    {
        Parser parser = new(_bufferLe);
        MsbtReader reader = new(ref parser);
        foreach (MsbtLabelSection.MsbtLabel label in reader.LabelSection) {
            _ = label.Index;
        }
        foreach (MsbtAttributeSection.MsbtAttribute atr in reader.AttributeSection) {
            _ = atr.Index;
        }
    }

    [Benchmark]
    public void ParseBE()
    {
        Parser parser = new(_bufferBe);
        MsbtReader reader = new(ref parser);
        foreach (MsbtLabelSection.MsbtLabel label in reader.LabelSection) {
            _ = label.Index;
        }
        foreach (MsbtAttributeSection.MsbtAttribute atr in reader.AttributeSection) {
            _ = atr.Index;
        }
    }
}
