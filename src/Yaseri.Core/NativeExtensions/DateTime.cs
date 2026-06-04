using System;
using System.Globalization;

namespace Yaseri;

file class Hints
{
	public static PrimitiveTypeHint TypeHint = new()
	{
		TypeName = "System.DateTime",
		Type = typeof(DateTime),
	};
}

public partial interface IPrimitiveReader
{
	bool TryReadValue(out DateTime value)
	{
		value = default;

		ValueHint(Hints.TypeHint);
		if (!TryReadValue(out string valueStr))
		{
			LastError = "Expected string";
			return false;
		}

		if (!DateTime.TryParseExact(valueStr, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out value))
		{
			LastError = "Failed to parse datetime";
			return false;
		}

		return true;
	}
}

public partial interface IPrimitiveWriter
{
	void WriteValue(DateTime value)
	{
		ValueHint(Hints.TypeHint);
		WriteValue(value.ToString("O"));
	}
}
