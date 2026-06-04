using System;
using System.Collections.Generic;

namespace Yaseri;

file static class Helper<T>
{
	public readonly static PrimitiveTypeHint TypeHint = new()
	{
		TypeName = "System.Collections.Generic.IReadOnlyList",
		Type = typeof(IReadOnlyList<T>),
	};

	public static bool TryReadGenericList(
		IPrimitiveReader reader,
		out IReadOnlyList<T> value,
		Func<IPrimitiveReader, (bool success, T val)> readValue)
	{
		reader.ValueHint(TypeHint);
		if (!reader.TryReadStartArray())
		{
			value = Array.Empty<T>();
			reader.LastError = "Expected array start";
			return false;
		}

		var ret = new List<T>();
		while (!reader.TryReadEndArray())
		{
			var (success, item) = readValue(reader);
			if (!success)
			{
				value = Array.Empty<T>();
				return false;
			}

			ret.Add(item);
		}

		value = ret;
		return true;
	}

	public static void WriteGenericList(
		IPrimitiveWriter writer,
		in IReadOnlyList<T> value,
		Action<IPrimitiveWriter, T> writeValue)
	{
		writer.ValueHint(TypeHint);
		writer.WriteStartArray();
		foreach (var item in value)
			writeValue(writer, item);
		writer.WriteEndArray();
	}
}

public partial interface IPrimitiveReader
{
	bool TryReadValue<T>(out IReadOnlyList<T> value)
		where T : IPrimitive<T>
	{
		ValueHint(Helper<T>.TypeHint);
		if (!TryReadStartArray())
		{
			value = Array.Empty<T>();
			LastError = "Expected array start";
			return false;
		}

		var ret = new List<T>();
		while (!TryReadEndArray())
		{
			if (!TryReadValue(out T item))
			{
				value = Array.Empty<T>();
				return false;
			}

			ret.Add(item);
		}

		value = ret;
		return true;
	}

	bool TryReadValue(out IReadOnlyList<bool> value) =>
		Helper<bool>.TryReadGenericList(this, out value, static (reader) =>
		{
			bool success = reader.TryReadValue(out bool value);
			return (success, value);
		});

	bool TryReadValue(out IReadOnlyList<sbyte> value) =>
		Helper<sbyte>.TryReadGenericList(this, out value, static (reader) =>
		{
			bool success = reader.TryReadValue(out sbyte value);
			return (success, value);
		});
	bool TryReadValue(out IReadOnlyList<int> value) =>
		Helper<int>.TryReadGenericList(this, out value, static (reader) =>
		{
			bool success = reader.TryReadValue(out int value);
			return (success, value);
		});
	bool TryReadValue(out IReadOnlyList<long> value) =>
		Helper<long>.TryReadGenericList(this, out value, static (reader) =>
		{
			bool success = reader.TryReadValue(out long value);
			return (success, value);
		});
	bool TryReadValue(out IReadOnlyList<byte> value) =>
		Helper<byte>.TryReadGenericList(this, out value, static (reader) =>
		{
			bool success = reader.TryReadValue(out byte value);
			return (success, value);
		});

	bool TryReadValue(out IReadOnlyList<ushort> value) =>
		Helper<ushort>.TryReadGenericList(this, out value, static (reader) =>
		{
			bool success = reader.TryReadValue(out ushort value);
			return (success, value);
		});

	bool TryReadValue(out IReadOnlyList<uint> value) =>
		Helper<uint>.TryReadGenericList(this, out value, static (reader) =>
		{
			bool success = reader.TryReadValue(out uint value);
			return (success, value);
		});

	bool TryReadValue(out IReadOnlyList<ulong> value) =>
		Helper<ulong>.TryReadGenericList(this, out value, static (reader) =>
		{
			bool success = reader.TryReadValue(out ulong value);
			return (success, value);
		});

	bool TryReadValue(out IReadOnlyList<float> value) =>
		Helper<float>.TryReadGenericList(this, out value, static (reader) =>
		{
			bool success = reader.TryReadValue(out float value);
			return (success, value);
		});
	bool TryReadValue(out IReadOnlyList<double> value) =>
		Helper<double>.TryReadGenericList(this, out value, static (reader) =>
		{
			bool success = reader.TryReadValue(out double value);
			return (success, value);
		});

	bool TryReadValue(out IReadOnlyList<string> value) =>
		Helper<string>.TryReadGenericList(this, out value, static (reader) =>
		{
			bool success = reader.TryReadValue(out string value);
			return (success, value);
		});
	bool TryReadValue(out IReadOnlyList<byte[]> value) =>
		Helper<byte[]>.TryReadGenericList(this, out value, static (reader) =>
		{
			bool success = reader.TryReadValue(out byte[] value);
			return (success, value);
		});
}

public partial interface IPrimitiveWriter
{
	void WriteValue<T>(in IReadOnlyList<T> value)
		where T : IPrimitive<T>
	{
		ValueHint(Helper<T>.TypeHint);
		WriteStartArray();
		foreach (var item in value)
			WriteValue(item);
		WriteEndArray();
	}

	void WriteValue(in IReadOnlyList<bool> value) => Helper<bool>.WriteGenericList(this, value, static (writer, value) => writer.WriteValue(value));

	void WriteValue(in IReadOnlyList<sbyte> value) => Helper<sbyte>.WriteGenericList(this, value, static (writer, value) => writer.WriteValue(value));
	void WriteValue(in IReadOnlyList<short> value) => Helper<short>.WriteGenericList(this, value, static (writer, value) => writer.WriteValue(value));
	void WriteValue(in IReadOnlyList<int> value) => Helper<int>.WriteGenericList(this, value, static (writer, value) => writer.WriteValue(value));
	void WriteValue(in IReadOnlyList<long> value) => Helper<long>.WriteGenericList(this, value, static (writer, value) => writer.WriteValue(value));

	void WriteValue(in IReadOnlyList<byte> value) => Helper<byte>.WriteGenericList(this, value, static (writer, value) => writer.WriteValue(value));
	void WriteValue(in IReadOnlyList<ushort> value) => Helper<ushort>.WriteGenericList(this, value, static (writer, value) => writer.WriteValue(value));
	void WriteValue(in IReadOnlyList<uint> value) => Helper<uint>.WriteGenericList(this, value, static (writer, value) => writer.WriteValue(value));
	void WriteValue(in IReadOnlyList<ulong> value) => Helper<ulong>.WriteGenericList(this, value, static (writer, value) => writer.WriteValue(value));

	void WriteValue(in IReadOnlyList<float> value) => Helper<float>.WriteGenericList(this, value, static (writer, value) => writer.WriteValue(value));
	void WriteValue(in IReadOnlyList<double> value) => Helper<double>.WriteGenericList(this, value, static (writer, value) => writer.WriteValue(value));

	void WriteValue(in IReadOnlyList<string> value) => Helper<string>.WriteGenericList(this, value, static (writer, value) => writer.WriteValue(value));
	void WriteValue(in IReadOnlyList<byte[]> value) => Helper<byte[]>.WriteGenericList(this, value, static (writer, value) => writer.WriteValue(value));
}
