using System;

namespace Yaseri.Binson;

internal static class BinsonConstants
{
	// Note that the magic bytes intentionally contains a 0x01 and 0x00 bytes to prevent the
	// sequence from being interpreted as a UTF-8 encoded file
	public static readonly byte[] MagicBytes = "B\x01ns\x00n"u8.ToArray();
}

public enum BinsonOpcodes : byte
{
	LeadingMagic = (byte)'B', // followed by 0x01 'n' 's' 0x00 'n'
	Null = (byte)'?',
	True = (byte)'t',
	False = (byte)'f',
	StartArray = (byte)'[',
	EndArray = (byte)']',
	StartObject = (byte)'{',
	Key = (byte)':',
	EndObject = (byte)'}',
	Utf8String = (byte)'s',
	ByteArray = (byte)'b',
	Zero = (byte)'0',
	One = (byte)'1',
	PositiveInt = (byte)'i',
	NegativeInt = (byte)'I',
	PositiveFloat16 = (byte)'h',
	NegativeFloat16 = (byte)'H',
	PositiveFloat32 = (byte)'f',
	NegativeFloat32 = (byte)'F',
	PositiveFloat64 = (byte)'d',
	NegativeFloat64 = (byte)'D',
}
