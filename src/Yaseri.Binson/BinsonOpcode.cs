using System;

namespace Yaseri.Binson;

internal static class BinsonConstants
{
	// Note that the magic bytes intentionally contains a 0x01 and 0x00 bytes to prevent the
	// sequence from being interpreted as a UTF-8 encoded file
	public static readonly byte[] MagicBytes = new byte[] { 0xB1, (byte)'n', (byte)'s', 0, (byte)'n', };
}

public enum BinsonOpcode : byte
{
	LeadingMagic = 0xB1, // followed by 'n' 's' 0x00 'n'
	Null = (byte)'?',
	True = (byte)'+',
	False = (byte)'-',
	StartArray = (byte)'[',
	EndArray = (byte)']',
	StartObject = (byte)'{',
	Key = (byte)':',
	EndObject = (byte)'}',
	Utf8String = (byte)'"',
	ByteArray = (byte)'#',
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
