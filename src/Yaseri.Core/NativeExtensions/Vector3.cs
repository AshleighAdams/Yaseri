using System.Numerics;

namespace Yaseri;

file class TypeNames
{
	public static string Vector3 { get; } = "System.Numerics.Vector3";
}

public partial interface IPrimitiveReader
{
	bool TryReadValue(out Vector3 value)
	{
		value = new Vector3();

		NextValueHint(PrimitiveHintType.Type, TypeNames.Vector3);
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
		NextValueHint(PrimitiveHintType.Type, TypeNames.Vector3);
		WriteStartArray(writeInline: true);
		WriteValue(value.X);
		WriteValue(value.Y);
		WriteValue(value.Z);
		WriteEndArray();
	}
}
