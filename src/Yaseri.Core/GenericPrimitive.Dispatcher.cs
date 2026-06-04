using System;

namespace Yaseri;

public partial class GenericPrimitive
{
	private static readonly GenericPrimitive Instance = new();

	private class Dispatcher<T> : IGenericPrimitive<T>
		where T : new()
	{
		public static readonly IGenericPrimitive<T> Specialized = Instance as IGenericPrimitive<T> ?? new Dispatcher<T>();
		public static readonly IPrimitive<T>? Primitive = new T() as IPrimitive<T>;
		public bool TryReadValue(IPrimitiveReader reader, out T value)
		{
			throw new NotSupportedException($"GenericPrimitive does not support {typeof(T).Name}");
		}

		public void WriteValue(IPrimitiveWriter writer, in T value)
		{
			throw new NotSupportedException($"GenericPrimitive does not support {typeof(T).Name}");
		}
	}

	public static bool TryReadValue<T>(IPrimitiveReader reader, out T value)
		where T : new()
	{
		if (Dispatcher<T>.Primitive is not null)
			return Dispatcher<T>.Primitive.InstancedTryReadValue(reader, out value);
		return Dispatcher<T>.Specialized.TryReadValue(reader, out value);
	}

	public static void WriteValue<T>(IPrimitiveWriter writer, in T value)
		where T : new()
	{
		if (Dispatcher<T>.Primitive is not null)
		{
			Dispatcher<T>.Primitive.InstancedWriteValue(writer, in value);
			return;
		}
		Dispatcher<T>.Specialized.WriteValue(writer, in value);
	}
}
