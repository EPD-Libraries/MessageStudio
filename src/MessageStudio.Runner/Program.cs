#if RELEASE
BenchmarkDotNet.Running.BenchmarkRunner.Run<MessageStudio.Runner.Benchmarks.MsbtParserBenchmarks>();
#else

using MessageStudio.Core.Common;
using MessageStudio.Core.Formats.BinaryText;
using MessageStudio.Core.Formats.BinaryText.Structures.Sections;
using System.Text;

byte[] buffer = File.ReadAllBytes(args[0]);
Parser parser = new(buffer);
MsbtReader reader = new(ref parser);

Console.WriteLine($"Magic: {Encoding.UTF8.GetString(reader.Header.Magic)}");
Console.WriteLine($"Byte Order Mark: {reader.Header.ByteOrderMark}");
Console.WriteLine($"Encoding: {reader.Header.Encoding}");
Console.WriteLine($"Version: {reader.Header.Version}");
Console.WriteLine($"Section Count: {reader.Header.SectionCount}");
Console.WriteLine($"File Size: {reader.Header.FileSize}");

Console.WriteLine("\nLabels:");
foreach (MsbtLabelSection.MsbtLabel label in reader.LabelSection) {
    Console.WriteLine($"{label.Index}: {label.Value}");
}

Console.WriteLine("\nAttributes:");
foreach (MsbtAttributeSection.MsbtAttribute atr in reader.AttributeSection) {
    Console.WriteLine($"{atr.Index}: {atr.Value}");
}

Console.WriteLine("\nText:");
foreach (MsbtTextSection.MsbtText txt in reader.TextSection) {
    Console.WriteLine($"{txt.Index}: {txt.Value}");
}

#endif