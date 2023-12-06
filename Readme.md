# Message Studio C#

[![License](https://img.shields.io/badge/License-AGPL%20v3.0-blue.svg)](License.txt) [![Downloads](https://img.shields.io/github/downloads/EPD-Libraries/MessageStudio/total)](https://github.com/EPD-Libraries/MessageStudio/releases)

Modern implementation of (some) EPD MessageStudio file formats written in managed C#

## Current Support

- MSBT (**M**essage **S**tudio **B**inary **T**ext)

### MSBT Usage

> From Binary
```cs
byte[] data = File.ReadAllBytes("path/to/file.msbt");
Msbt msbt = Msbt.FromBinary(data);
```

> From Binary (Constructor)
```cs
byte[] data = File.ReadAllBytes("path/to/file.msbt");
Msbt msbt = new(data);
```

> To Binary
```cs
/* ... */

using MemoryStream ms = new();
msbt.ToBinary(ms,
    endianness: Endian.Little,
    encoding: Encoding.Unicode // Encoding.UTF8 is not supported!
);
```

> To Yaml
```cs
/* ... */

string yaml = msbt.ToYaml();

// For performance, use ReadOnlyMsbt
byte[] data = File.ReadAllBytes("path/to/file.msbt");
ReadOnlyMsbt readOnlyMsbt = new(data);
string yaml = readOnlyMsbt.ToYaml();
```