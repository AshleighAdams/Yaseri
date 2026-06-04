using System;
using System.Drawing;
using System.Numerics;

namespace Yaseri;

file class Hints
{
	public static PrimitiveTypeHint TypeHint = new()
	{
		TypeName = "System.Numerics.Vector3",
		Type = typeof(DateTime),
	};
}

public partial interface IPrimitiveReader
{
	bool TryReadValue(out Vector3 value)
	{
		value = new Vector3();

		ValueHint(Hints.TypeHint);
		if (!TryReadStartArray())
		{
			LastError = "Expected array start for Vector3";
			return false;
		}

		if (!TryReadValue(out value.X))
		{
			LastError = "Failed to read X";
			return false;
		}

		if (!TryReadValue(out value.Y))
		{
			LastError = "Failed to read Y";
			return false;
		}

		if (!TryReadValue(out value.Z))
		{
			LastError = "Failed to read Y";
			return false;
		}

		if (!TryReadEndArray(skipToEnd: false))
		{
			LastError = "Expected array end for Vector3";
			return false;
		}

		return true;
	}
}

public partial interface IPrimitiveWriter
{
	void WriteValue(Vector3 value)
	{
		ValueHint(Hints.TypeHint);
		WriteStartArray(writeInline: true);
		WriteValue(value.X);
		WriteValue(value.Y);
		WriteValue(value.Z);
		WriteEndArray();
	}
}

public partial class GenericPrimitive : IGenericPrimitive<Vector3>
{
	bool IGenericPrimitive<Vector3>.TryReadValue(IPrimitiveReader reader, out Vector3 value) => reader.TryReadValue(out value);
	void IGenericPrimitive<Vector3>.WriteValue(IPrimitiveWriter writer, in Vector3 value) => writer.WriteValue(value);
}
