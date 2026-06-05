namespace Yaseri;

file static class Helper<T>
	where T : struct
{
	public readonly static PrimitiveTypeHint TypeHint = new()
	{
		TypeName = "System.Nullable",
		Type = typeof(T?),
	};
}

public partial interface IPrimitiveReader
{
	bool TryReadValue<T>(out T? value)
		where T : struct
	{
		ValueHint(Helper<T>.TypeHint);
		if (TryReadNull())
		{
			value = null;
			return true;
		}

		if (!GenericPrimitive.TryReadValue(this, out T item))
		{
			value = null;
			return false;
		}

		value = item;
		return true;
	}
}

public partial interface IPrimitiveWriter
{
	void WriteValue<T>(in T? value)
		where T : struct
	{
		ValueHint(Helper<T>.TypeHint);
		if (value.HasValue)
			GenericPrimitive.WriteValue(this, value.Value);
		else
			WriteNull();
	}
}
