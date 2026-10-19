using System.Collections.Generic;
using System.Text;
using System.Text.Unicode;

namespace Yaseri;

// TODO: this should use an OrderedDictionary<TKey, TValue> when .NET is upgraded to >= 9

file static class Helper<T>
{
	public readonly static PrimitiveTypeHint TypeHint = new()
	{
		TypeName = "System.Collections.Generic.IReadOnlyDictionary",
		Type = typeof(IReadOnlyDictionary<string, T>),
	};
}

file class Encodings
{
	public readonly static UTF8Encoding Utf8 = new(encoderShouldEmitUTF8Identifier: false);
}

public partial interface IPrimitiveReader
{
	bool TryReadValue<T>(out IReadOnlyDictionary<string, T> value)
		where T : new()
	{
		ValueHint(Helper<T>.TypeHint);
		if (!TryReadStartObject())
		{
			value = new SortedDictionary<string, T>();
			LastError = "Expected array start";
			return false;
		}

		var ret = new Dictionary<string, T>();
		while (!TryReadEndObject())
		{
			if (!TryReadKey(out var key))
			{
				value = new SortedDictionary<string, T>();
				LastError = "Expected key";
				return false;
			}

			if (!GenericPrimitive.TryReadValue(this, out T item))
			{
				value = new SortedDictionary<string, T>();
				return false;
			}

			var keytStr = Encodings.Utf8.GetString(key.Span);
			if (!ret.TryAdd(keytStr, item))
			{
				value = new SortedDictionary<string, T>();
				LastError = "Duplicate key found";
				return false;
			}
		}

		value = ret;
		return true;
	}
}

public partial interface IPrimitiveWriter
{
	void WriteValue<T>(in IReadOnlyDictionary<string, T> value)
		where T : new()
	{
		ValueHint(Helper<T>.TypeHint);
		WriteStartObject();
		foreach (var (k, v) in value)
		{
			WriteKey(Encodings.Utf8.GetBytes(k));
			GenericPrimitive.WriteValue(this, v);
		}
		WriteEndObject();
	}
}
