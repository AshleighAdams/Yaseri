using System.Diagnostics.CodeAnalysis;

namespace Yaseri;

[SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Static interfaces")]
public interface IPrimitive<T>
{
	static abstract bool TryReadValue(IPrimitiveReader reader, out T value);
	static abstract void WriteValue(IPrimitiveWriter writer, in T value);
}
