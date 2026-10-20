using System;
using System.Collections.Generic;
using System.Drawing;

namespace Yaseri;

file static class Helper<T>
{
	public readonly static PrimitiveTypeHint TypeHint = new()
	{
		TypeName = "System.Collections.Generic.IReadOnlyList",
		Type = typeof(IReadOnlyList<T>),
	};
}

public partial interface IPrimitiveReader
{
	bool TryReadValue(out IReadOnlyList<string> value)
	{
		ValueHint(Helper<string>.TypeHint);
		if (!TryReadStartArray())
		{
			value = Array.Empty<string>();
			LastError = "Expected array start";
			return false;
		}

		var ret = new List<string>();
		while (!TryReadEndArray())
		{
			if (!TryReadValue(out string item))
			{
				value = Array.Empty<string>();
				return false;
			}

			ret.Add(item);
		}

		value = ret;
		return true;
	}

	bool TryReadValue<T>(out IReadOnlyList<T> value)
		where T : new()
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
			if (!GenericPrimitive.TryReadValue(this, out T item))
			{
				value = Array.Empty<T>();
				return false;
			}

			ret.Add(item);
		}

		value = ret;
		return true;
	}
}

public partial interface IPrimitiveWriter
{
	void WriteValue(in IReadOnlyList<string> value)
	{
		ValueHint(Helper<string>.TypeHint);
		WriteStartArray();
		foreach (var item in value)
			WriteValue(item);
		WriteEndArray();
	}

	void WriteValue<T>(in IReadOnlyList<T> value)
		where T : new()
	{
		ValueHint(Helper<T>.TypeHint);
		WriteStartArray();
		foreach (var item in value)
			GenericPrimitive.WriteValue(this, item);
		WriteEndArray();
	}
}
