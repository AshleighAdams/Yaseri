using System;

namespace Yaseri.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class FormatAttribute : Attribute
{
	public FormatAttribute(string key)
	{
		Key = key;
	}

	public string Key { get; }
}
