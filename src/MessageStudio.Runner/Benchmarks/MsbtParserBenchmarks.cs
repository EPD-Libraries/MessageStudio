using BenchmarkDotNet.Attributes;
using MessageStudio.Core.Common;
using MessageStudio.Core.Formats.Msbt;
using MessageStudio.Core.Formats.Msbt.Structures.Sections;

namespace MessageStudio.Runner.Benchmarks;

[MemoryDiagnoser(true)]
public class MsbtParserBenchmarks
{
    private byte[] _buffer = [];

    [GlobalSetup]
    public void Setup()
    {
        _buffer = File.ReadAllBytes("D:\\bin\\Msbt\\100enemy-LE.msbt");
    }

    [Benchmark]
    public void Parse()
    {
        Parser parser = new(_buffer);
        MsbtReader reader = new(ref parser);
        foreach (MsbtLabelSection.MsbtLabel label in reader.LabelSection) {
            int index = label.Index;
            string value = label.Value;
        }
    }
}
