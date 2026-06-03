using System.Drawing;
using System.Numerics;

namespace Yaseri;

file class TypeNames
{
	public static string Color { get; } = "System.Drawing.Color";
}

public partial interface IPrimitiveReader
{
	bool TryReadValue(out Color value)
	{
		value = new Color();

		NextValueHint(PrimitiveHintType.Type, TypeNames.Color);
		if (!TryReadValue(out string colorStr))
		{
			LastError = "Failed to read color string";
			return false;
		}

		try
		{
			value = ColorTranslator.FromHtml(colorStr);
		}
		catch
		{
			LastError = "Failed to parse color string";
			return false;
		}
		return true;
	}
}

public partial interface IPrimitiveWriter
{
	void WriteValue(Color value)
	{
		NextValueHint(PrimitiveHintType.Type, TypeNames.Color);
		WriteValue(ColorTranslator.ToHtml(value));
	}
}
