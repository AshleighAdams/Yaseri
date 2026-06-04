using System.Collections.Generic;

namespace Yaseri;

public sealed record PrimitiveUsageHint
{
	// Constraints
	public object? MinimumConstraint { get; init; }
	public object? MaximumConstraint { get; init; }
	public object? StepConstraint { get; init; }
	// User defined
	public ISet<string>? CustomHints { get; init; }
}
