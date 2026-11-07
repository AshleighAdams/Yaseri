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
	bool TryReadValue<T>(out IReadOnlyList<T> value)
	{
		var nullable = IsNullable<T>.Value;

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
			T item;
			if (nullable && TryReadNull())
				item = default!;
			else if (!GenericPrimitive.TryReadValue(this, out item))
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
	void WriteValue<T>(in IReadOnlyList<T> value)
	{
		var nullable = IsNullable<T>.Value;
		ValueHint(Helper<T>.TypeHint);
		WriteStartArray();
		foreach (var item in value)
			if (nullable && item is null)
				WriteNull();
			else
				GenericPrimitive.WriteValue(this, item);
		WriteEndArray();
	}
}
