using System;

namespace Yaseri.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class UsageAttribute : Attribute
{
	public UsageAttribute(string hint)
	{
		Hint = hint;
	}

	public string Hint { get; }
}
