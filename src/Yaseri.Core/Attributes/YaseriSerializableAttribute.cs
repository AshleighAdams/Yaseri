using System;

namespace Yaseri.Attributes;

[AttributeUsage(
	AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum,
	Inherited = false,
	AllowMultiple = false)]
public sealed class YaseriSerializableAttribute : Attribute
{
	public YaseriSerializableAttribute()
	{
	}
}
