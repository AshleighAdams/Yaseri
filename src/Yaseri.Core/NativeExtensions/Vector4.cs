using System;
using System.Numerics;

namespace Yaseri;

file class Hints
{
	public static PrimitiveTypeHint TypeHint = new()
	{
		TypeName = "System.Numerics.Vector4",
		Type = typeof(DateTime),
	};
}

public partial interface IPrimitiveReader
{
	bool TryReadValue(out Vector4 value)
	{
		value = new Vector4();

		ValueHint(Hints.TypeHint);
		if (!TryReadStartArray())
		{
			LastError = "Expected array start for Vector4";
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
			LastError = "Failed to read Z";
			return false;
		}

		if (!TryReadValue(out value.W))
		{
			LastError = "Failed to read W";
			return false;
		}

		if (!TryReadEndArray(skipToEnd: false))
		{
			LastError = "Expected array end for Vector4";
			return false;
		}

		return true;
	}
}

public partial interface IPrimitiveWriter
{
	void WriteValue(Vector4 value)
	{
		ValueHint(Hints.TypeHint);
		WriteStartArray(writeInline: true);
		WriteValue(value.X);
		WriteValue(value.Y);
		WriteValue(value.Z);
		WriteValue(value.W);
		WriteEndArray();
	}
}
