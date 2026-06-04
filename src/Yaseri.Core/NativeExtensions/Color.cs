using System.Drawing;

namespace Yaseri;

file class Hints
{
	public static PrimitiveTypeHint TypeHint = new()
	{
		TypeName = "System.Drawing.Color",
		Type = typeof(Color),
	};
}

public partial interface IPrimitiveReader
{
	bool TryReadValue(out Color value)
	{
		value = new Color();

		ValueHint(Hints.TypeHint);
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
		ValueHint(Hints.TypeHint);
		WriteValue(ColorTranslator.ToHtml(value));
	}
}
