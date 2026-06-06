using System;
using System.Text;

namespace Yaseri.Binson;

public record struct BinsonPrimitiveReaderOptions
{
	public bool SkipMagicBytesCheck { get; set; }
}

public class BinsonPrimitiveReader : IPrimitiveReader
{
	private readonly UTF8Encoding Utf8 = new(encoderShouldEmitUTF8Identifier: false);

	private BinsonPrimitiveReaderOptions Options { get; }
	private ReadOnlyMemory<byte> Content { get; }
	private string Filename { get; }
	private int Depth { get; set; }

	public BinsonPrimitiveReader(ReadOnlyMemory<byte> source, string filename, BinsonPrimitiveReaderOptions? options = default)
	{
		Content = source;
		Filename = filename;
		Options = options ?? new();
	}

	public int CurrentPosition { get; private set; }
	long IPrimitiveReader.CurrentPosition => CurrentPosition;

	public string? LastError { get; set; }

	private bool TryReadMagic()
	{
		int remaining = Content.Length - CurrentPosition;
		if (remaining < BinsonConstants.MagicBytes.Length)
		{
			LastError = "Magic bytes truncated";
			return false;
		}

		if (!Options.SkipMagicBytesCheck)
		{
			var magicGood = Content[CurrentPosition..BinsonConstants.MagicBytes.Length].Span.SequenceEqual(BinsonConstants.MagicBytes);
			if (!magicGood)
			{
				LastError = "Trailing magic bytes did not match expected value.";
				return false;
			}
		}

		CurrentPosition += BinsonConstants.MagicBytes.Length;
		return true;
	}

	public bool TryReadOpcode(BinsonOpcode opcode)
	{
		if (CurrentPosition >= Content.Length)
		{
			LastError = "Unexpected EOF";
			return false;
		}

		byte current = Content.Span[CurrentPosition];
		if (current != (byte)opcode)
		{
			if (current == (byte)BinsonOpcode.LeadingMagic && TryReadMagic())
				return TryReadOpcode(opcode);
			else
				return false;
		}

		CurrentPosition++;
		return true;
	}

	public bool TryPeekOpcode(out BinsonOpcode opcode)
	{
		if (CurrentPosition >= Content.Length)
		{
			LastError = "Unexpected EOF";
			opcode = default;
			return false;
		}

		opcode = (BinsonOpcode)Content.Span[CurrentPosition];
		return true;
	}

	private bool TryPeekLength(out int length, out int bytesRead, int offset = 0)
	{
		offset += CurrentPosition;
		bytesRead = VarInt.ReadPositiveInteger(Content.Span[offset..], out var value);
		if (bytesRead == -1)
		{
			LastError = "Expected varint";
			length = default;
			return false;
		}

		if (value > int.MaxValue)
		{
			LastError = "Varint value too large";
			length = default;
			return false;
		}

		length = (int)value;
		return true;
	}

	private bool TryReadUInt(ulong maxValue, out ulong value)
	{
		if (!TryPeekOpcode(out var opcode))
		{
			LastError = "Expected encoded number, got EOF";
			value = default;
			return false;
		}

		switch (opcode)
		{
			case BinsonOpcode.Zero:
				CurrentPosition++;
				value = 0;
				return true;
			case BinsonOpcode.One:
				CurrentPosition++;
				value = 1;
				return true;
			case BinsonOpcode.NegativeInt:
				LastError = "Expected positive number, got negative";
				value = default;
				return false;
			case BinsonOpcode.PositiveInt:
				CurrentPosition++;
				var read = VarInt.ReadPositiveInteger(Content.Span[CurrentPosition..], out var unsignedValue);
				if (read == -1)
				{
					LastError = "Expected encoded number";
					value = default;
					return false;
				}
				if (unsignedValue > maxValue)
				{
					LastError = "Integer too large";
					value = default;
					return false;
				}
				value = unsignedValue;
				CurrentPosition += read;
				return true;
			default:
				LastError = "Expected encoded number";
				value = default;
				return false;
		}
	}

	private bool TryReadInt(long minValue, long maxValue, out long value)
	{
		if (!TryPeekOpcode(out var opcode))
		{
			LastError = "Expected encoded number, got EOF";
			value = default;
			return false;
		}

		bool negate = false;
		switch (opcode)
		{
			case BinsonOpcode.Zero:
				CurrentPosition++;
				value = 0;
				return true;
			case BinsonOpcode.One:
				CurrentPosition++;
				value = 1;
				return true;
			case BinsonOpcode.NegativeInt:
				negate = true;
				goto case BinsonOpcode.PositiveInt;
			case BinsonOpcode.PositiveInt:
				CurrentPosition++;
				var read = VarInt.ReadPositiveInteger(Content.Span[CurrentPosition..], out var unsignedValue);
				if (read == -1)
				{
					LastError = "Expected encoded number";
					value = default;
					return false;
				}
				if (unsignedValue > (ulong)maxValue)
				{
					LastError = "Integer too large";
					value = default;
					return false;
				}
				var ret = (long)unsignedValue;
				if (negate)
					ret = -ret;
				if (ret < minValue)
				{
					LastError = "Integer too small";
					value = default;
					return false;
				}
				value = ret;
				CurrentPosition += read;
				return true;
			default:
				LastError = "Expected encoded number";
				value = default;
				return false;
		}
	}

	private bool TryReadFloat(double minValue, double maxValue, out double value)
	{
		if (!TryPeekOpcode(out var opcode))
		{
			LastError = "Expected encoded number, got EOF";
			value = default;
			return false;
		}

		bool negate = false;
		int read;
		double floatValue = 0.0;
		switch (opcode)
		{
			case BinsonOpcode.Zero:
				CurrentPosition++;
				value = 0;
				return true;
			case BinsonOpcode.One:
				CurrentPosition++;
				value = 1;
				return true;
			case BinsonOpcode.NegativeInt:
				negate = true;
				goto case BinsonOpcode.PositiveInt;
			case BinsonOpcode.PositiveInt:
				CurrentPosition++;
				read = VarInt.ReadPositiveInteger(Content.Span[CurrentPosition..], out var unsignedValue);
				if (read == -1)
				{
					LastError = "Expected encoded number";
					value = default;
					return false;
				}
				if (unsignedValue > maxValue)
				{
					LastError = "Float (integer) too large";
					value = default;
					return false;
				}
				var ret = (double)unsignedValue;
				if (negate)
					ret = -ret;
				if (ret < minValue)
				{
					LastError = "Float (integer) too small";
					value = default;
					return false;
				}
				value = ret;
				CurrentPosition += read;
				return true;

			case BinsonOpcode.NegativeFloat16:
				negate = true;
				goto case BinsonOpcode.PositiveFloat64;
			case BinsonOpcode.PositiveFloat16:
				CurrentPosition++;
				read = VarFloat.ReadPositiveFloat16(Content.Span[CurrentPosition..], out var float16Value);
				floatValue = (double)float16Value;
				goto check_float;

			case BinsonOpcode.NegativeFloat32:
				negate = true;
				goto case BinsonOpcode.PositiveFloat64;
			case BinsonOpcode.PositiveFloat32:
				CurrentPosition++;
				read = VarFloat.ReadPositiveFloat32(Content.Span[CurrentPosition..], out var float32Value);
				floatValue = (double)float32Value;
				goto check_float;

			case BinsonOpcode.NegativeFloat64:
				negate = true;
				goto case BinsonOpcode.PositiveFloat64;
			case BinsonOpcode.PositiveFloat64:
				CurrentPosition++;
				read = VarFloat.ReadPositiveFloat64(Content.Span[CurrentPosition..], out floatValue);
				goto check_float;

			check_float:
				if (read == -1)
				{
					LastError = "Expected encoded float";
					value = default;
					return false;
				}
				if (negate)
					floatValue = -floatValue;
				if (floatValue > maxValue)
				{
					LastError = "Float too large";
					value = default;
					return false;
				}
				if (floatValue < minValue)
				{
					LastError = "Float too small";
					value = default;
					return false;
				}

				CurrentPosition += read;
				value = floatValue;
				return true;
			default:
				LastError = "Expected encoded number";
				value = default;
				return false;
		}
	}

	private bool SkipThisValue()
	{
		int depth = 1;
		int read;

		while (true)
		{
			if (!TryPeekOpcode(out var opcode))
				return false;

			switch (opcode)
			{
				case BinsonOpcode.StartObject:
				case BinsonOpcode.StartArray:
					CurrentPosition++;
					depth++;
					continue;

				case BinsonOpcode.EndObject:
				case BinsonOpcode.EndArray:
					CurrentPosition++;
					depth--;
					if (depth == 0)
						return true;
					continue;

				case BinsonOpcode.Null:
				case BinsonOpcode.True:
				case BinsonOpcode.False:
				case BinsonOpcode.Zero:
				case BinsonOpcode.One:
					CurrentPosition++;
					continue;

				case BinsonOpcode.Key:
				case BinsonOpcode.Utf8String:
				case BinsonOpcode.ByteArray:
					CurrentPosition++;
					read = VarInt.ReadPositiveInteger(Content.Span[CurrentPosition..], out var length);
					if (read == -1)
					{
						LastError = "Expected encoded number for length";
						return false;
					}
					CurrentPosition += read + (int)length;
					continue;

				case BinsonOpcode.PositiveInt:
				case BinsonOpcode.PositiveFloat16:
				case BinsonOpcode.PositiveFloat32:
				case BinsonOpcode.PositiveFloat64:
				case BinsonOpcode.NegativeInt:
				case BinsonOpcode.NegativeFloat16:
				case BinsonOpcode.NegativeFloat32:
				case BinsonOpcode.NegativeFloat64:
					CurrentPosition++;
					read = VarInt.ReadPositiveInteger(Content.Span[CurrentPosition..], out _);
					if (read == -1)
					{
						LastError = "Expected encoded number";
						return false;
					}
					CurrentPosition += read;
					continue;

				default:
					LastError = $"Unknown or unexpected  opcode {(int)opcode:X}, desync'd?";
					return false;
			}
		}
	}

	public bool TryReadStartObject()
	{
		if (!TryReadOpcode(BinsonOpcode.StartObject))
		{
			LastError = "Expected start object opcode";
			return false;
		}

		return true;
	}

	public bool TryReadKey(ReadOnlySpan<byte> expectedKey)
	{
		if (!TryPeekOpcode(out var opcode) || opcode != BinsonOpcode.StartObject)
		{
			LastError = "Expected start object opcode";
			return false;
		}

		if (!TryPeekLength(out var length, out var bytesRead, offset: 1))
			return false;

		var span = Content.Span[(CurrentPosition + 1 + bytesRead)..length];
		if (!span.SequenceEqual(expectedKey))
		{
			LastError = "Key does not match (contents)";
			return false;
		}

		CurrentPosition += 1 + bytesRead + length;
		return true;
	}

	public bool TryReadKey(out ReadOnlyMemory<byte> key)
	{
		if (!TryReadOpcode(BinsonOpcode.Key))
		{
			LastError = "Expected key opcode";
			key = Array.Empty<byte>();
			return false;
		}

		if (!TryPeekLength(out var length, out var bytesRead, offset: 0))
		{
			key = Array.Empty<byte>();
			return false;
		}

		key = Content.Slice(CurrentPosition + bytesRead, length);
		CurrentPosition += bytesRead + length;
		return true;
	}

	public bool TryReadEndObject(bool skipToEnd = false)
	{
		if (!TryReadOpcode(BinsonOpcode.EndObject))
		{
			LastError = "Expected end object opcode";
			return false;
		}

		return true;
	}

	public bool TryReadStartArray()
	{
		if (!TryReadOpcode(BinsonOpcode.StartArray))
		{
			LastError = "Expected start array opcode";
			return false;
		}

		return true;
	}

	public bool TryReadEndArray(bool skipToEnd = false)
	{
		if (!TryReadOpcode(BinsonOpcode.EndArray))
		{
			LastError = "Expected end array opcode";
			return false;
		}

		return true;
	}

	public bool TrySkipValue()
	{
		if (!TryPeekOpcode(out var opcode))
			return false;

		int read;
		switch (opcode)
		{
			case BinsonOpcode.Null:
			case BinsonOpcode.True:
			case BinsonOpcode.False:
			case BinsonOpcode.Zero:
			case BinsonOpcode.One:
				CurrentPosition++;
				break;

			case BinsonOpcode.Key:
			case BinsonOpcode.Utf8String:
			case BinsonOpcode.ByteArray:
				CurrentPosition++;
				read = VarInt.ReadPositiveInteger(Content.Span[CurrentPosition..], out var length);
				if (read == -1)
				{
					LastError = "Expected encoded number for length";
					return false;
				}
				CurrentPosition += read + (int)length;
				break;

			case BinsonOpcode.PositiveInt:
			case BinsonOpcode.PositiveFloat16:
			case BinsonOpcode.PositiveFloat32:
			case BinsonOpcode.PositiveFloat64:
			case BinsonOpcode.NegativeInt:
			case BinsonOpcode.NegativeFloat16:
			case BinsonOpcode.NegativeFloat32:
			case BinsonOpcode.NegativeFloat64:
				CurrentPosition++;
				read = VarInt.ReadPositiveInteger(Content.Span[CurrentPosition..], out _);
				if (read == -1)
				{
					LastError = "Expected encoded number for length";
					return false;
				}
				CurrentPosition += read;
				break;

			case BinsonOpcode.StartArray:
			case BinsonOpcode.StartObject:
				CurrentPosition++;
				if (!SkipThisValue())
					return false;

				if (!TryPeekOpcode(out var endOpcode))
					return false;
				if (opcode == BinsonOpcode.StartObject && endOpcode != BinsonOpcode.EndObject)
				{
					LastError = "Expected end object opcode";
					return false;
				}
				else if (opcode == BinsonOpcode.StartArray && endOpcode != BinsonOpcode.EndArray)
				{
					LastError = "Expected end array opcode";
					return false;
				}
				CurrentPosition++;
				break;
			default:
				LastError = "Unexpected token type";
				return false;
		}

		return true;
	}

	public bool TryReadNull()
	{
		if (!TryReadOpcode(BinsonOpcode.Null))
		{
			LastError = "Expected null opcode";
			return false;
		}

		return true;
	}

	public bool TryReadValue(out bool value)
	{
		if (!TryPeekOpcode(out var opcode) || opcode is not BinsonOpcode.True and not BinsonOpcode.False)
		{
			LastError = "Expected true or false opcode";
			value = default;
			return false;
		}

		CurrentPosition++;
		value = opcode == BinsonOpcode.True;
		return true;
	}

	public bool TryReadValue(out sbyte value)
	{
		if (!TryReadInt(sbyte.MinValue, sbyte.MaxValue, out var readInt))
		{
			value = default;
			return false;
		}

		value = (sbyte)readInt;
		return true;
	}

	public bool TryReadValue(out short value)
	{
		if (!TryReadInt(short.MinValue, short.MaxValue, out var readInt))
		{
			value = default;
			return false;
		}

		value = (short)readInt;
		return true;
	}

	public bool TryReadValue(out int value)
	{
		if (!TryReadInt(int.MinValue, int.MaxValue, out var readInt))
		{
			value = default;
			return false;
		}

		value = (int)readInt;
		return true;
	}

	public bool TryReadValue(out long value)
	{
		return TryReadInt(int.MinValue, int.MaxValue, out value);
	}

	public bool TryReadValue(out byte value)
	{
		if (!TryReadUInt(byte.MaxValue, out var readInt))
		{
			value = default;
			return false;
		}

		value = (byte)readInt;
		return true;
	}

	public bool TryReadValue(out ushort value)
	{
		if (!TryReadUInt(ushort.MaxValue, out var readInt))
		{
			value = default;
			return false;
		}

		value = (ushort)readInt;
		return true;
	}

	public bool TryReadValue(out uint value)
	{
		if (!TryReadUInt(uint.MaxValue, out var readInt))
		{
			value = default;
			return false;
		}

		value = (uint)readInt;
		return true;
	}

	public bool TryReadValue(out ulong value)
	{
		return TryReadUInt(ulong.MaxValue, out value);
	}

	public bool TryReadValue(out float value)
	{
		if (!TryReadFloat(float.MinValue, float.MaxValue, out var readFloat))
		{
			value = default;
			return false;
		}

		value = (float)readFloat;
		return true;
	}

	public bool TryReadValue(out double value)
	{
		return TryReadFloat(double.MinValue, double.MaxValue, out value);
	}

	public bool TryReadValue(out string value)
	{
		if (!TryReadOpcode(BinsonOpcode.Utf8String))
		{
			LastError = "Expected utf8 string opcode";
			value = string.Empty;
			return false;
		}

		if (!TryPeekLength(out var length, out var bytesRead, offset: 0))
		{
			value = string.Empty;
			return false;
		}

		var strSpan = Content.Span.Slice(CurrentPosition + bytesRead, length);
		CurrentPosition += bytesRead + length;
		value = Utf8.GetString(strSpan);
		return true;
	}

	public bool TryReadValue(out byte[] value)
	{
		if (!TryReadOpcode(BinsonOpcode.ByteArray))
		{
			LastError = "Expected utf8 string opcode";
			value = Array.Empty<byte>();
			return false;
		}


		if (!TryPeekLength(out var length, out var bytesRead, offset: 0))
		{
			value = Array.Empty<byte>();
			return false;
		}

		value = Content.Span[(CurrentPosition + bytesRead)..length].ToArray();
		CurrentPosition += bytesRead + length;
		return true;
	}
}
