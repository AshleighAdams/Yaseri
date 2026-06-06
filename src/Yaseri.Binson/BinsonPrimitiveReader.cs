using System;

namespace Yaseri.Binson;

public class BinsonPrimitiveReader : IPrimitiveReader
{
	public long CurrentPosition => throw new NotImplementedException();

	public string? LastError { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

	public bool TryReadEndArray(bool skipToEnd = false)
	{
		throw new NotImplementedException();
	}

	public bool TryReadEndObject(bool skipToEnd = false)
	{
		throw new NotImplementedException();
	}

	public bool TryReadKey(ReadOnlySpan<byte> expectedKey)
	{
		throw new NotImplementedException();
	}

	public bool TryReadKey(out ReadOnlyMemory<byte> key)
	{
		throw new NotImplementedException();
	}

	public bool TryReadNull()
	{
		throw new NotImplementedException();
	}

	public bool TryReadStartArray()
	{
		throw new NotImplementedException();
	}

	public bool TryReadStartObject()
	{
		throw new NotImplementedException();
	}

	public bool TryReadValue(out bool value)
	{
		throw new NotImplementedException();
	}

	public bool TryReadValue(out sbyte value)
	{
		throw new NotImplementedException();
	}

	public bool TryReadValue(out short value)
	{
		throw new NotImplementedException();
	}

	public bool TryReadValue(out int value)
	{
		throw new NotImplementedException();
	}

	public bool TryReadValue(out long value)
	{
		throw new NotImplementedException();
	}

	public bool TryReadValue(out byte value)
	{
		throw new NotImplementedException();
	}

	public bool TryReadValue(out ushort value)
	{
		throw new NotImplementedException();
	}

	public bool TryReadValue(out uint value)
	{
		throw new NotImplementedException();
	}

	public bool TryReadValue(out ulong value)
	{
		throw new NotImplementedException();
	}

	public bool TryReadValue(out float value)
	{
		throw new NotImplementedException();
	}

	public bool TryReadValue(out double value)
	{
		throw new NotImplementedException();
	}

	public bool TryReadValue(out string value)
	{
		throw new NotImplementedException();
	}

	public bool TryReadValue(out byte[] value)
	{
		throw new NotImplementedException();
	}

	public bool TrySkipValue()
	{
		throw new NotImplementedException();
	}
}
