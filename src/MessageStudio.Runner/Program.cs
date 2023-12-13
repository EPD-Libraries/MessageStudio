#if RELEASE

using BenchmarkDotNet.Running;
using MessageStudio.Runner.Benchmarks;

BenchmarkRunner.Run([typeof(ImmutableMsbtBenchmarks), typeof(MsbtReadBenchmarks), typeof(MsbtWriteBenchmarks)]);

#else

// using MessageStudio.Formats.BinaryText;
// using MessageStudio.Formats.BinaryText.Structures;
// using MessageStudio.Formats.BinaryText.Structures.Sections;
// using SarcLibrary;
// using System.Collections.Generic;

//byte[] buffer = File.ReadAllBytes(args[0]);
//Msbt msbt = new(buffer);

//Console.WriteLine($"Magic: {MsbtHeader.Magic}");
//Console.WriteLine($"Byte Order Mark: {msbt.ReadOnly.Header.ByteOrderMark}");
//Console.WriteLine($"Encoding: {msbt.ReadOnly.Header.Encoding}");
//Console.WriteLine($"Version: {msbt.ReadOnly.Header.Version}");
//Console.WriteLine($"Section Count: {msbt.ReadOnly.Header.SectionCount}");
//Console.WriteLine($"File Size: {msbt.ReadOnly.Header.FileSize}");

//Console.WriteLine("\nLabels:");
//foreach (MsbtLabel label in msbt.ReadOnly.LabelSection) {
//    Console.WriteLine($"{label.Index}: {label.Value}");
//}

//if (msbt.ReadOnly.AttributeSection is not null) {
//    Console.WriteLine("\nAttributes:");
//    foreach (MsbtAttribute atr in msbt.ReadOnly.AttributeSection) {
//        Console.WriteLine($"{atr.Index}: {atr.Value}");
//    }
//}

//Console.WriteLine("\nText:");
//foreach (MsbtText txt in msbt.ReadOnly.TextSection) {
//    Console.WriteLine($"{txt.Index}: {txt.Value}");
//}

//File.WriteAllText("D:\\bin\\Msbt\\test.yml", msbt.ReadOnly.ToYaml());

using MessageStudio.Formats.BinaryText;
using Revrs;
using SarcLibrary;
using System.Diagnostics;

Stopwatch stopwatch = Stopwatch.StartNew();

foreach (var file in Directory.GetFiles("D:\\bin\\Msbt\\Mals")) {
    byte[] data = File.ReadAllBytes(file);
    RevrsReader reader = new(data);
    ImmutableSarc sarc = new(ref reader);

    foreach ((var name, var buffer) in sarc) {
        Msbt msbt = Msbt.FromBinary(buffer);
        foreach ((var label, var entry) in msbt) {
            string text = entry.Text + entry.Attribute ?? string.Empty;
            if (string.IsNullOrEmpty(text)) {
                continue;
            }
        }

        string path = Path.Combine(
            "D:\\bin\\Msbt\\Mals-Yaml",
            Path.GetFileName(file),
            Path.GetDirectoryName(name) ?? string.Empty,
            Path.GetFileNameWithoutExtension(name) + ".yml");

        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
        File.WriteAllText(path, msbt.ToYaml());
    }
}

stopwatch.Stop();
Console.WriteLine($"{stopwatch.ElapsedMilliseconds}ms");

#endif