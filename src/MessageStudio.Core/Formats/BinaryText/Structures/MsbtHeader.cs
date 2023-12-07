﻿using MessageStudio.Core.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MessageStudio.Core.Formats.BinaryText.Structures;

[StructLayout(LayoutKind.Explicit, Size = 32)]
public readonly struct MsbtHeader
{
    [FieldOffset(0x00)]
    public readonly ulong Magic;

    [FieldOffset(0x08)]
    public readonly Endian ByteOrderMark;

    [FieldOffset(0x0C)]
    public readonly Encoding Encoding;

    [FieldOffset(0x0D)]
    public readonly byte Version;

    [FieldOffset(0x0E)]
    public readonly ushort SectionCount;

    [FieldOffset(0x12)]
    public readonly uint FileSize;

    public MsbtHeader() { }
    public MsbtHeader(ulong magic, Endian byteOrderMark, Encoding encoding, byte version, ushort sectionCount, uint fileSize)
    {
        Magic = magic;
        ByteOrderMark = byteOrderMark;
        Encoding = encoding;
        Version = version;
        SectionCount = sectionCount;
        FileSize = fileSize;
    }

    public class Reverser : ISpanReverser
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Reverse(in Span<byte> buffer)
        {
            buffer[0x08..0x0A].Reverse();
            buffer[0x0E..0x10].Reverse();
            buffer[0x12..0x16].Reverse();
        }
    }
}
