using System;

namespace Yaseri;

public partial interface IPrimitiveReader
{
	long CurrentPosition { get; }
	string? LastError { get; set; }

	void ValueHint(PrimitiveTypeHint typeHint) {}
	void ValueHint(PrimitiveUsageHint usageHint) {}
	void ValueHint(string format) {}

	bool TryReadStartObject();
	bool TryReadKey(ReadOnlySpan<byte> expectedKey);
	bool TryReadKey(out ReadOnlyMemory<byte> key);
	bool TryReadEndObject(bool skipToEnd = false);

	bool TryReadStartArray();
	bool TryReadEndArray(bool skipToEnd = false);

	bool TrySkipValue();
	bool TryReadNull();

	bool TryReadValue(out bool value);

	bool TryReadValue(out sbyte value);
	bool TryReadValue(out short value);
	bool TryReadValue(out int value);
	bool TryReadValue(out long value);

	bool TryReadValue(out byte value);
	bool TryReadValue(out ushort value);
	bool TryReadValue(out uint value);
	bool TryReadValue(out ulong value);

	bool TryReadValue(out float value);
	bool TryReadValue(out double value);

	bool TryReadValue(out string value);
	bool TryReadValue(out byte[] value);

	bool TryReadValue<T>(out T value)
		where T : IPrimitive<T>
	{
		return T.TryReadValue(this, out value);
	}
};
