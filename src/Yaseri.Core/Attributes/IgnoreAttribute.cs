using System;

namespace Yaseri.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class IgnoreAttribute : Attribute
{
}
