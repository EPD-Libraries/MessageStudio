#if RELEASE
BenchmarkDotNet.Running.BenchmarkRunner.Run<MessageStudio.Runner.Benchmarks.MsbtParserBenchmarks>();
#else

using MessageStudio.Core.Formats.BinaryText;
using MessageStudio.Core.Formats.BinaryText.Structures;
using MessageStudio.Core.Formats.BinaryText.Structures.Sections;

byte[] buffer = File.ReadAllBytes(args[0]);
Msbt msbt = Msbt.FromBinary(buffer);

Console.WriteLine($"Magic: {MsbtHeader.Magic}");
Console.WriteLine($"Byte Order Mark: {msbt.ReadOnly.Header.ByteOrderMark}");
Console.WriteLine($"Encoding: {msbt.ReadOnly.Header.Encoding}");
Console.WriteLine($"Version: {msbt.ReadOnly.Header.Version}");
Console.WriteLine($"Section Count: {msbt.ReadOnly.Header.SectionCount}");
Console.WriteLine($"File Size: {msbt.ReadOnly.Header.FileSize}");

Console.WriteLine("\nLabels:");
foreach (MsbtLabel label in msbt.ReadOnly.LabelSection) {
    Console.WriteLine($"{label.Index}: {label.Value}");
}

Console.WriteLine("\nAttributes:");
foreach (MsbtAttribute atr in msbt.ReadOnly.AttributeSection!) {
    Console.WriteLine($"{atr.Index}: {atr.Value}");
}

Console.WriteLine("\nText:");
foreach (MsbtText txt in msbt.ReadOnly.TextSection) {
    Console.WriteLine($"{txt.Index}: {txt.Value}");
}

File.WriteAllText("D:\\bin\\Msbt\\test.yml", msbt.ReadOnly.ToYaml());

#endif