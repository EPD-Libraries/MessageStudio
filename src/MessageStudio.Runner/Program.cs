#if RELEASE

using BenchmarkDotNet.Running;
using MessageStudio.Runner.Benchmarks;

BenchmarkRunner.Run([typeof(ImmutableMsbtBenchmarks), typeof(MsbtReadBenchmarks), typeof(MsbtWriteBenchmarks)]);

#else

// using MessageStudio.Formats.BinaryText;
// 
// byte[] buffer = File.ReadAllBytes(args[0]);
// 
// Msbt msbt = Msbt.FromBinary(buffer);
// string yaml = msbt.ToYaml();
// 
// Msbt yamlMsbt = Msbt.FromYaml(yaml);
// Console.WriteLine(yaml + "\n\n");
// Console.WriteLine(yaml == yamlMsbt.ToYaml());

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

        string yamlPath = Path.Combine(
            "D:\\bin\\Msbt\\Mals-Yaml",
            Path.GetFileName(file),
            Path.GetDirectoryName(name) ?? string.Empty,
            Path.GetFileNameWithoutExtension(name) + ".yml");

        string yaml = msbt.ToYaml();
        Directory.CreateDirectory(Path.GetDirectoryName(yamlPath) ?? string.Empty);
        File.WriteAllText(yamlPath, yaml);

        Msbt yamlMsbt = Msbt.FromYaml(yaml);

        string binaryPath = Path.Combine(
            "D:\\bin\\Msbt\\Mals-Yaml",
            Path.GetFileName(file),
            Path.GetDirectoryName(name) ?? string.Empty,
            Path.GetFileNameWithoutExtension(name) + ".msbt");

        Directory.CreateDirectory(Path.GetDirectoryName(binaryPath) ?? string.Empty);
        using FileStream fs = File.Create(binaryPath);
        yamlMsbt.WriteBinary(fs, msbt.Encoding, msbt.Endianness);
    }
}

stopwatch.Stop();
Console.WriteLine($"{stopwatch.ElapsedMilliseconds}ms");

#endif