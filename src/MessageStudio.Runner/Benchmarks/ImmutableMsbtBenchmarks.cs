using BenchmarkDotNet.Attributes;
using MessageStudio.Formats.BinaryText;
using Revrs;

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
        RevrsReader reader = new(_bufferLE);
        ImmutableMsbt _ = new(ref reader);
    }

    [Benchmark]
    public void Read_BE()
    {
        RevrsReader reader = new(_bufferBE);
        ImmutableMsbt _ = new(ref reader);
    }
}
