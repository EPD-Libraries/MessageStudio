using MessageStudio.Core.Common;
using MessageStudio.Core.Formats.Msbt;
using MessageStudio.Core.Formats.Msbt.Structures.Sections;
using System.Text;

// BenchmarkDotNet.Running.BenchmarkRunner.Run<MessageStudio.Runner.Benchmarks.MsbtParserBenchmarks>();
// return;

byte[] buffer = File.ReadAllBytes(args[0]);
Parser parser = new(buffer);
MsbtReader reader = new(ref parser);

Console.WriteLine($"Magic: {Encoding.UTF8.GetString(reader.Header.Magic)}");
Console.WriteLine($"Byte Order Mark: {reader.Header.ByteOrderMark}");
Console.WriteLine($"Encoding: {reader.Header.Encoding}");
Console.WriteLine($"Version: {reader.Header.Version}");
Console.WriteLine($"Section Count: {reader.Header.SectionCount}");
Console.WriteLine($"File Size: {reader.Header.FileSize}");

foreach (MsbtLabelSection.MsbtLabel label in reader.LabelSection) {
    Console.WriteLine($"{label.Index}: {label.Value}");
}