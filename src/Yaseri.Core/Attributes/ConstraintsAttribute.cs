using System;
using System.Collections.Generic;

namespace Yaseri.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class ConstraintsAttribute : Attribute
{
	public object? Minimum { get; init; }
	public object? Maximum { get; init; }
	public object? Step { get; init; }
}
