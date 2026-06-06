using System;
using System.IO;
using System.Text;

namespace Yaseri.Binson;

public record struct BinsonPrimitiveWriterOptions
{
	public BinsonPrimitiveWriterOptions() {}
	public bool EmitMagic { get; set; } = true;
	public bool BiasVariableInts { get; set; } = true;
}

public class BinsonPrimitiveWriter : IPrimitiveWriter
{
	private MemoryStream Stream = new();
	public BinsonPrimitiveWriter(BinsonPrimitiveWriterOptions? options = default)
	{
		var opts = options ?? new();
		if (opts.EmitMagic)
			Stream.Write(BinsonConstants.MagicBytes);
	}

	public long CurrentPosition => (int)Stream.Position;

	public bool ShouldWriteDefaultValues { get; set; }

	public void WriteEndArray() => Stream.WriteByte((byte)BinsonOpcode.EndArray);
	public void WriteEndObject() => Stream.WriteByte((byte)BinsonOpcode.EndObject);

	public void WriteKey(ReadOnlySpan<byte> key)
	{
		Span<byte> buffer = stackalloc byte[VarInt.MaxBufferLength];
		int wroteBytes = 0;
		wroteBytes = VarInt.WritePositiveInteger((uint)key.Length, buffer);
		Stream.WriteByte((byte)BinsonOpcode.Key);
		Stream.Write(buffer[..wroteBytes]);
		Stream.Write(key);
	}

	public void WriteNull() => Stream.WriteByte((byte)BinsonOpcode.Null);
	public void WriteStartArray(bool writeInline = false) => Stream.WriteByte((byte)BinsonOpcode.StartArray);
	public void WriteStartObject(bool writeInline = false) => Stream.WriteByte((byte)BinsonOpcode.StartObject);
	public void WriteValue(bool value) => Stream.WriteByte((byte)(value ? BinsonOpcode.True : BinsonOpcode.False));


	public void WriteValue(sbyte value)
	{
		if (value == 0)
		{
			Stream.WriteByte((byte)BinsonOpcode.Zero);
			return;
		}
		else if (value == 1)
		{
			Stream.WriteByte((byte)BinsonOpcode.One);
			return;
		}

		Span<byte> buffer = stackalloc byte[VarInt.MaxBufferLength];
		int wroteBytes = 0;
		if (value >= 0)
		{
			wroteBytes = VarInt.WritePositiveInteger((byte)value, buffer);
			Stream.WriteByte((byte)BinsonOpcode.PositiveInt);
		}
		else
		{
			wroteBytes = VarInt.WritePositiveInteger(1 + (uint)~value, buffer);
			Stream.WriteByte((byte)BinsonOpcode.NegativeInt);
		}
		Stream.Write(buffer[..wroteBytes]);
	}

	public void WriteValue(short value)
	{
		if (value == 0)
		{
			Stream.WriteByte((byte)BinsonOpcode.Zero);
			return;
		}
		else if (value == 1)
		{
			Stream.WriteByte((byte)BinsonOpcode.One);
			return;
		}

		Span<byte> buffer = stackalloc byte[VarInt.MaxBufferLength];
		int wroteBytes = 0;
		if (value >= 0)
		{
			wroteBytes = VarInt.WritePositiveInteger((ushort)value, buffer);
			Stream.WriteByte((byte)BinsonOpcode.PositiveInt);
		}
		else
		{
			wroteBytes = VarInt.WritePositiveInteger(1 + (uint)~value, buffer);
			Stream.WriteByte((byte)BinsonOpcode.NegativeInt);
		}
		Stream.Write(buffer[..wroteBytes]);
	}

	public void WriteValue(int value)
	{
		if (value == 0)
		{
			Stream.WriteByte((byte)BinsonOpcode.Zero);
			return;
		}
		else if (value == 1)
		{
			Stream.WriteByte((byte)BinsonOpcode.One);
			return;
		}

		Span<byte> buffer = stackalloc byte[VarInt.MaxBufferLength];
		int wroteBytes = 0;
		if (value >= 0)
		{
			wroteBytes = VarInt.WritePositiveInteger((uint)value, buffer);
			Stream.WriteByte((byte)BinsonOpcode.PositiveInt);
		}
		else
		{
			wroteBytes = VarInt.WritePositiveInteger(1 + (uint)~value, buffer);
			Stream.WriteByte((byte)BinsonOpcode.NegativeInt);
		}
		Stream.Write(buffer[..wroteBytes]);
	}

	public void WriteValue(long value)
	{
		if (value == 0)
		{
			Stream.WriteByte((byte)BinsonOpcode.Zero);
			return;
		}
		else if (value == 1)
		{
			Stream.WriteByte((byte)BinsonOpcode.One);
			return;
		}

		Span<byte> buffer = stackalloc byte[VarInt.MaxBufferLength];
		int wroteBytes = 0;
		if (value >= 0)
		{
			wroteBytes = VarInt.WritePositiveInteger((ulong)value, buffer);
			Stream.WriteByte((byte)BinsonOpcode.PositiveInt);
		}
		else
		{
			wroteBytes = VarInt.WritePositiveInteger(1 + (ulong)~value, buffer);
			Stream.WriteByte((byte)BinsonOpcode.NegativeInt);
		}
		Stream.Write(buffer[..wroteBytes]);
	}

	public void WriteValue(byte value)
	{
		if (value == 0)
		{
			Stream.WriteByte((byte)BinsonOpcode.Zero);
			return;
		}
		else if (value == 1)
		{
			Stream.WriteByte((byte)BinsonOpcode.One);
			return;
		}

		Span<byte> buffer = stackalloc byte[VarInt.MaxBufferLength];
		int wroteBytes = 0;
		wroteBytes = VarInt.WritePositiveInteger(value, buffer);
		Stream.WriteByte((byte)BinsonOpcode.PositiveInt);
		Stream.Write(buffer[..wroteBytes]);
	}

	public void WriteValue(ushort value)
	{
		if (value == 0)
		{
			Stream.WriteByte((byte)BinsonOpcode.Zero);
			return;
		}
		else if (value == 1)
		{
			Stream.WriteByte((byte)BinsonOpcode.One);
			return;
		}

		Span<byte> buffer = stackalloc byte[VarInt.MaxBufferLength];
		int wroteBytes = 0;
		Stream.WriteByte((byte)BinsonOpcode.PositiveInt);
		wroteBytes = VarInt.WritePositiveInteger(value, buffer);
		Stream.Write(buffer[..wroteBytes]);
	}

	public void WriteValue(uint value)
	{
		if (value == 0)
		{
			Stream.WriteByte((byte)BinsonOpcode.Zero);
			return;
		}
		else if (value == 1)
		{
			Stream.WriteByte((byte)BinsonOpcode.One);
			return;
		}

		Span<byte> buffer = stackalloc byte[VarInt.MaxBufferLength];
		int wroteBytes = 0;
		wroteBytes = VarInt.WritePositiveInteger(value, buffer);
		Stream.WriteByte((byte)BinsonOpcode.PositiveInt);
		Stream.Write(buffer[..wroteBytes]);
	}

	public void WriteValue(ulong value)
	{
		if (value == 0)
		{
			Stream.WriteByte((byte)BinsonOpcode.Zero);
			return;
		}
		else if (value == 1)
		{
			Stream.WriteByte((byte)BinsonOpcode.One);
			return;
		}

		Span<byte> buffer = stackalloc byte[VarInt.MaxBufferLength];
		int wroteBytes = 0;
		wroteBytes = VarInt.WritePositiveInteger(value, buffer);
		Stream.WriteByte((byte)BinsonOpcode.PositiveInt);
		Stream.Write(buffer[..wroteBytes]);
	}

	public void WriteValue(float value)
	{
		WriteValue((double)value);
	}

	public void WriteValue(double value)
	{
		Span<byte> buffer = stackalloc byte[VarFloat.MaxBufferLength + 1];
		int wroteExtra = 0;

		bool isNegative = value < 0.0;
		value = Math.Abs(value);

		if (value == 0.0)
		{
			buffer[0] = (byte)BinsonOpcode.Zero;
			wroteExtra = 0;
		}
		else if (value == 1.0)
		{
			buffer[0] = (byte)BinsonOpcode.One;
			wroteExtra = 0;
		}
		else if (value == (double)(Half)value)
		{
			buffer[0] = isNegative ? (byte)BinsonOpcode.NegativeFloat16 : (byte)BinsonOpcode.PositiveFloat16;
			wroteExtra = VarFloat.WritePositiveFloat16((Half)value, buffer[1..]);
		}
		else if (value == (double)(float)value)
		{
			buffer[0] = isNegative ? (byte)BinsonOpcode.NegativeFloat32 : (byte)BinsonOpcode.PositiveFloat32;
			wroteExtra = VarFloat.WritePositiveFloat32((float)value, buffer[1..]);
		}
		else
		{
			buffer[0] = isNegative ? (byte)BinsonOpcode.NegativeFloat64 : (byte)BinsonOpcode.PositiveFloat64;
			wroteExtra = VarFloat.WritePositiveFloat64(value, buffer[1..]);
		}

		if (wroteExtra < 0)
			throw new Exception("Could not encode float");
		Stream.Write(buffer[..(wroteExtra + 1)]);
	}

	public void WriteValue(char value)
	{
		WriteValue(value.ToString());
	}

	private readonly UTF8Encoding Utf8 = new(encoderShouldEmitUTF8Identifier: false);
	public void WriteValue(string value)
	{
		var valueBytes = Utf8.GetBytes(value);
		Span<byte> buffer = stackalloc byte[VarInt.MaxBufferLength];
		int wroteBytes = 0;
		wroteBytes = VarInt.WritePositiveInteger((uint)valueBytes.Length, buffer);
		Stream.WriteByte((byte)BinsonOpcode.Utf8String);
		Stream.Write(buffer[..wroteBytes]);
		Stream.Write(valueBytes);
	}

	public void WriteValue(byte[] value)
	{
		Span<byte> buffer = stackalloc byte[VarInt.MaxBufferLength];
		int wroteBytes = 0;
		wroteBytes = VarInt.WritePositiveInteger((uint)value.Length, buffer);
		Stream.WriteByte((byte)BinsonOpcode.ByteArray);
		Stream.Write(buffer[..wroteBytes]);
		Stream.Write(value);
	}

	public Memory<byte> GetContent()
	{
		Stream.Flush();
		return Stream.ToArray();
	}
}
