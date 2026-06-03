using System;
using System.Globalization;

namespace Yaseri;

file class TypeNames
{
	public static string DateTime { get; } = "System.DateTime";
}

public partial interface IPrimitiveReader
{
	bool TryReadValue(out DateTime value)
	{
		value = default;

		NextValueHint(PrimitiveHintType.Type, TypeNames.DateTime);
		if (!TryReadValue(out string valueStr))
		{
			LastError = "Failed to read color string";
			return false;
		}

		if (!DateTime.TryParseExact(valueStr, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out value))
		{
			LastError = "Failed to parse datetime string";
			return false;
		}

		return true;
	}
}

public partial interface IPrimitiveWriter
{
	void WriteValue(DateTime value)
	{
		NextValueHint(PrimitiveHintType.Type, TypeNames.DateTime);
		WriteValue(value.ToString("O"));
	}
}
