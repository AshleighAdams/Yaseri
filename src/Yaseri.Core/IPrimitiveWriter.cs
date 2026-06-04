using System;

namespace Yaseri;

public partial interface IPrimitiveWriter
{
	long CurrentPosition { get; }
	bool ShouldWriteDefaultValues { get; }

	void ValueHint(PrimitiveTypeHint hint) { }
	void ValueHint(PrimitiveUsageHint hint) { }
	void ValueHint(string format) { }

	void WriteStartObject(bool writeInline = false);
	void WriteKey(ReadOnlySpan<byte> key);
	void WriteEndObject();

	void WriteStartArray(bool writeInline = false);
	void WriteEndArray();

	void WriteNull();
	void WriteValue(bool value);

	void WriteValue(sbyte value);
	void WriteValue(short value);
	void WriteValue(int value);
	void WriteValue(long value);

	void WriteValue(byte value);
	void WriteValue(ushort value);
	void WriteValue(uint value);
	void WriteValue(ulong value);

	void WriteValue(float value);
	void WriteValue(double value);

	void WriteValue(char value);
	void WriteValue(string value);

	void WriteValue(byte[] value);

	void WriteValue<T>(in T value)
		where T : IPrimitive<T>
	{
		T.WriteValue(this, value);
	}
};
