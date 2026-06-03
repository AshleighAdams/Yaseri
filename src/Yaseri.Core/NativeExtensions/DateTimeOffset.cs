using System;
using System.Globalization;

namespace Yaseri;

file class TypeNames
{
	public static string DateTimeOffset { get; } = "System.DateTimeOffset";
}

public partial interface IPrimitiveReader
{
	bool TryReadValue(out DateTimeOffset value)
	{
		value = default;

		NextValueHint(PrimitiveHintType.Type, TypeNames.DateTimeOffset);
		if (!TryReadValue(out string valueStr))
		{
			LastError = "Failed to read color string";
			return false;
		}

		if (!DateTimeOffset.TryParseExact(valueStr, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out value))
		{
			LastError = "Failed to parse datetime string";
			return false;
		}

		return true;
	}
}

public partial interface IPrimitiveWriter
{
	void WriteValue(DateTimeOffset value)
	{
		NextValueHint(PrimitiveHintType.Type, TypeNames.DateTimeOffset);
		WriteValue(value.ToString("O"));
	}
}
