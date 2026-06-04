using System;

namespace Yaseri.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class PropertyNameAttribute : Attribute
{
	public PropertyNameAttribute(string key)
	{
		Key = key;
	}

	public string Key { get; }
}
