using System;
using System.Collections.Generic;

namespace Yaseri;

public sealed record PrimitiveTypeHint
{
	public required string TypeName { get; init; }
	public Type? Type { get; init; }
	public bool EnumIsFlags { get; init; }
	public IDictionary<string, int>? EnumValues { get; init; }
}
