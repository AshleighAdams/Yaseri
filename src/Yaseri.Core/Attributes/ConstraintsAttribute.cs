using System;
using System.Collections.Generic;

namespace Yaseri.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class ConstraintsAttribute : Attribute
{
	public object? Minimum { get; init; }
	public object? Maximum { get; init; }
	public ISet<object>? Whitelist { get; init; }
	public ISet<object>? Blacklist { get; init; }
}
