using System;

namespace Yaseri;


public static class IsNullable<T>
{
	public static readonly bool Value = GetIsNullabile();
	private static bool GetIsNullabile()
	{
		var type = typeof(T);
		if (!type.IsValueType)
			return true;
		if (Nullable.GetUnderlyingType(type) != null)
			return true;
		return false;
	}
}
