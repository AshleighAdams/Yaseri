using System.Numerics;

namespace Yaseri;

file class TypeNames
{
	public static string Vector2 { get; } = "System.Numerics.Vector2";
}

public partial interface IPrimitiveReader
{
	bool TryReadValue(out Vector2 value)
	{
		value = new Vector2();

		NextValueHint(PrimitiveHintType.Type, TypeNames.Vector2);
		if (!TryReadStartArray())
		{
			LastError = "Expected array start for Vector2";
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

		if (!TryReadEndArray(skipToEnd: false))
		{
			LastError = "Expected array end for Vector2";
			return false;
		}

		return true;
	}
}

public partial interface IPrimitiveWriter
{
	void WriteValue(Vector2 value)
	{
		NextValueHint(PrimitiveHintType.Type, TypeNames.Vector2);
		WriteStartArray(writeInline: true);
		WriteValue(value.X);
		WriteValue(value.Y);
		WriteEndArray();
	}
}
