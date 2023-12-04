#if RELEASE
BenchmarkDotNet.Running.BenchmarkRunner.Run<MessageStudio.Runner.Benchmarks.MsbtParserBenchmarks>();
#else

using MessageStudio.Core.Common;
using MessageStudio.Core.Formats.BinaryText;
using MessageStudio.Core.Formats.BinaryText.Structures;
using MessageStudio.Core.Formats.BinaryText.Structures.Sections;

byte[] buffer = File.ReadAllBytes(args[0]);
MemoryReader parser = new(buffer);
MsbtReader msbt = new(parser);

Console.WriteLine($"Magic: {MsbtHeader.Magic}");
Console.WriteLine($"Byte Order Mark: {msbt.Header.ByteOrderMark}");
Console.WriteLine($"Encoding: {msbt.Header.Encoding}");
Console.WriteLine($"Version: {msbt.Header.Version}");
Console.WriteLine($"Section Count: {msbt.Header.SectionCount}");
Console.WriteLine($"File Size: {msbt.Header.FileSize}");

Console.WriteLine("\nLabels:");
foreach (MsbtLabel label in msbt.LabelSection) {
    Console.WriteLine($"{label.Index}: {label.Value}");
}

Console.WriteLine("\nAttributes:");
foreach (MsbtAttribute atr in msbt.AttributeSection!) {
    Console.WriteLine($"{atr.Index}: {atr.Value}");
}

Console.WriteLine("\nText:");
foreach (MsbtText txt in msbt.TextSection) {
    Console.WriteLine($"{txt.Index}: {txt.Value}");
}

File.WriteAllText("D:\\bin\\Msbt\\test.yml", msbt.ToYaml());

#endif