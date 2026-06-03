# Binson

## Opcodes

```cs
enum Opcodes : byte
{
	Null = '?',
	True = 't',
	False = 'f',
	StartArray '\[',
	EndArray = ']',
	StartObject = '{',
	Key = ':',
	EndObject = '}',
	Utf8String = 's',
	ByteArray = 'b',
	Zero = '0',
	One = '1',
	UInt = 'u',
	Int = 'i',
	Float16 = 'f',
	Float32 = 'F',
	Float64 = 'd',
}
```

## Sequences

```
VarIntByte =
	multibyte = int1
	data = int7

\# UTF-8 codepoint system, but for variable length integers
VarInt =
	data = VarIntByte\[1..until multibyte is 0]

Key =
	length = VarInt
	value = byte\[length]

String =
	length = VarInt
	value = byte\[length]

Int =
	value = 1 + VarInt

UInt =
	value = 1 + VarInt

Float16 =
	value = IEEE 754 binary16

Float32 =
	value = IEEE 754 binary32

Float64 =
	value = IEEE 754 binary64

```
