using BenchmarkDotNet.Attributes;
using MessageStudio.Core.Formats.BinaryText;
using MessageStudio.Core.IO;

namespace MessageStudio.Runner.Benchmarks;

[MemoryDiagnoser(true)]
public class ImmutableMsbtBenchmarks
{
    private byte[] _bufferLE = [];
    private byte[] _bufferBE = [];

    [GlobalSetup]
    public void Setup()
    {
        _bufferLE = File.ReadAllBytes("D:\\bin\\Msbt\\100enemy-LE.msbt");
        _bufferBE = File.ReadAllBytes("D:\\bin\\Msbt\\100enemy.msbt");
    }

    [Benchmark]
    public void Read_LE()
    {
        SpanReader reader = new(_bufferLE);
        ImmutableMsbt msbt = new(ref reader);
    }

    [Benchmark]
    public void Read_BE()
    {
        SpanReader reader = new(_bufferBE);
        ImmutableMsbt msbt = new(ref reader);
    }
}
