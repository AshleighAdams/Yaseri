#pragma description Binson
#pragma author Ashleigh Adams <ashleigh.adams@inseinc.com>
#pragma endian big
#pragma magic [ B1 6E 73 00 6E ] @ 0x00

//#pragma array_limit 0x100000
//#pragma pattern_limit 0x200000

#include <std/string.pat>
#include <std/sys.pat>

import type.magic;

fn CountLeadingOnes(u8 byte)
{
	u32 count = 0;
	for (s32 i = 7, i >= 0, i -= 1)
		if ((byte >> i) & 1)
			count += 1;
		else
			break;
	return count;
};

fn DecodeVarInt(u8 firstByte, ref u8 extraBytes, u32 extraBytesCount)
{
	u64 value = 0;
	u8 mask = 0xFF >> (extraBytesCount + 1);
	value = firstByte & mask;

	for (u32 i = 0, i < extraBytesCount, i += 1)
		value = (value << 8) | extraBytes[i];
	return value;
};

enum Opcode : u8
{
	LeadingMagic = 0xB1,
	Null = '?',
	True = '+',
	False = '-',
	StartArray = '[',
	EndArray = ']',
	StartObject = '{',
	Key = ':',
	EndObject = '}',
	Utf8String = '"',
	ByteArray = '#',
	Zero = '0',
	One = '1',
	PositiveInt = 'i',
	NegativeInt = 'I',
	PositiveFloat16 = 'h',
	NegativeFloat16 = 'H',
	PositiveFloat32 = 'f',
	NegativeFloat32 = 'F',
	PositiveFloat64 = 'd',
	NegativeFloat64 = 'D',
};

struct VarInt
{
	u8 FirstByte;
	u32 ExtraBytesCount = CountLeadingOnes(FirstByte);

	u8 ExtraBytes[ExtraBytesCount];
	u64 Value = DecodeVarInt(FirstByte, ExtraBytes, ExtraBytesCount);
} [[sealed, format("FormatVarInt")]];

struct VarFloat16
{
	VarInt;
} [[sealed, format("FormatVarFloat16")]];

struct VarFloat32
{
	VarInt;
} [[sealed, format("FormatVarFloat32")]];

struct VarFloat64
{
	VarInt;
} [[sealed, format("FormatVarFloat64")]];

struct String
{
	VarInt Size;
	char Data[Size.Value];
} [[format("FormatString")]];

struct ByteArray
{
	VarInt Size;
	u8 Value[Size.Value];
};

using BinsonValue;

struct Array
{
	BinsonValue Items[while(std::mem::read_unsigned($, 1) != Opcode::EndArray)];
	Opcode Terminator;
} [[inline]];

struct ObjectKeyValue
{
	Opcode Opcode;
	if (Opcode != Opcode::Key)
		std::error("Expected Key opcode");
	String Key;
	BinsonValue Value;
} [[format("FormatObjectKeyValue")]];

struct Object
{
	ObjectKeyValue Properties[while(std::mem::read_unsigned($, 1) != Opcode::EndObject)];
	Opcode Terminator;
}[[inline]];

struct BinsonValue
{
	Opcode Opcode;

	match (Opcode)
	{
		(Opcode::StartArray): Array Value;
		(Opcode::StartObject): Object Value;
		(Opcode::Utf8String): String Value;
		(Opcode::ByteArray): ByteArray Value;
		(Opcode::PositiveInt): VarInt Value;
		(Opcode::NegativeInt): VarInt Value;
		(Opcode::PositiveFloat16): VarFloat16 Value;
		(Opcode::NegativeFloat16): VarFloat16 Value;
		(Opcode::PositiveFloat32): VarFloat32 Value;
		(Opcode::NegativeFloat32): VarFloat32 Value;
		(Opcode::PositiveFloat64): VarFloat64 Value;
		(Opcode::NegativeFloat64): VarFloat64 Value;
	}
} [[format("FormatValue")]];

struct BinsonHeader
{
	type::Magic<"\xB1ns\x00n"> Magic;
	BinsonValue Value;
};

// format value helpers

fn FormatVarInt(VarInt varint) { return std::format("{}", varint.Value); };
fn FormatVarFloat16(VarFloat16 value) { return "?.?f"; };
fn FormatVarFloat32(VarFloat32 value) { return "?.?f"; };
fn FormatVarFloat64(VarFloat64 value) { return "?.?f"; };
fn FormatString(String value) { return std::format("\"{}\"", type::escape_bytes(value.Data)); };
fn FormatObjectKeyValue(ObjectKeyValue value)
{
	return std::format("{}: {}", value.Key, value.Value);
};
fn FormatValue(BinsonValue value)
{
	try
	{
		if (value.Opcode == Opcode::StartObject)
			return "Object";
		if (value.Opcode == Opcode::StartArray)
			return "Array";
		return std::string::to_string(value.Value);
	}
	catch
	{
		return std::string::to_string(value.Opcode);
	}
};

// root object
BinsonHeader binson @ 0x00;
