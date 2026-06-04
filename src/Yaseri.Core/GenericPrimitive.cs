using System;

namespace Yaseri;

internal interface IGenericPrimitive<T>
{
	bool TryReadValue(IPrimitiveReader reader, out T value);
	void WriteValue(IPrimitiveWriter writer, in T value);
}

public partial class GenericPrimitive :
	IGenericPrimitive<bool>,
	IGenericPrimitive<sbyte>,
	IGenericPrimitive<short>,
	IGenericPrimitive<int>,
	IGenericPrimitive<long>,
	IGenericPrimitive<byte>,
	IGenericPrimitive<ushort>,
	IGenericPrimitive<uint>,
	IGenericPrimitive<ulong>,
	IGenericPrimitive<float>,
	IGenericPrimitive<double>,
	IGenericPrimitive<string>,
	IGenericPrimitive<byte[]>
{
	bool IGenericPrimitive<bool>.TryReadValue(IPrimitiveReader reader, out bool value) => reader.TryReadValue(out value);
	bool IGenericPrimitive<sbyte>.TryReadValue(IPrimitiveReader reader, out sbyte value) => reader.TryReadValue(out value);
	bool IGenericPrimitive<short>.TryReadValue(IPrimitiveReader reader, out short value) => reader.TryReadValue(out value);
	bool IGenericPrimitive<int>.TryReadValue(IPrimitiveReader reader, out int value) => reader.TryReadValue(out value);
	bool IGenericPrimitive<long>.TryReadValue(IPrimitiveReader reader, out long value) => reader.TryReadValue(out value);
	bool IGenericPrimitive<byte>.TryReadValue(IPrimitiveReader reader, out byte value) => reader.TryReadValue(out value);
	bool IGenericPrimitive<ushort>.TryReadValue(IPrimitiveReader reader, out ushort value) => reader.TryReadValue(out value);
	bool IGenericPrimitive<uint>.TryReadValue(IPrimitiveReader reader, out uint value) => reader.TryReadValue(out value);
	bool IGenericPrimitive<ulong>.TryReadValue(IPrimitiveReader reader, out ulong value) => reader.TryReadValue(out value);
	bool IGenericPrimitive<float>.TryReadValue(IPrimitiveReader reader, out float value) => reader.TryReadValue(out value);
	bool IGenericPrimitive<double>.TryReadValue(IPrimitiveReader reader, out double value) => reader.TryReadValue(out value);
	bool IGenericPrimitive<string>.TryReadValue(IPrimitiveReader reader, out string value) => reader.TryReadValue(out value);
	bool IGenericPrimitive<byte[]>.TryReadValue(IPrimitiveReader reader, out byte[] value) => reader.TryReadValue(out value);

	void IGenericPrimitive<bool>.WriteValue(IPrimitiveWriter writer, in bool value) => writer.WriteValue(value);
	void IGenericPrimitive<sbyte>.WriteValue(IPrimitiveWriter writer, in sbyte value) => writer.WriteValue(value);
	void IGenericPrimitive<short>.WriteValue(IPrimitiveWriter writer, in short value) => writer.WriteValue(value);
	void IGenericPrimitive<int>.WriteValue(IPrimitiveWriter writer, in int value) => writer.WriteValue(value);
	void IGenericPrimitive<long>.WriteValue(IPrimitiveWriter writer, in long value) => writer.WriteValue(value);
	void IGenericPrimitive<byte>.WriteValue(IPrimitiveWriter writer, in byte value) => writer.WriteValue(value);
	void IGenericPrimitive<ushort>.WriteValue(IPrimitiveWriter writer, in ushort value) => writer.WriteValue(value);
	void IGenericPrimitive<uint>.WriteValue(IPrimitiveWriter writer, in uint value) => writer.WriteValue(value);
	void IGenericPrimitive<ulong>.WriteValue(IPrimitiveWriter writer, in ulong value) => writer.WriteValue(value);
	void IGenericPrimitive<float>.WriteValue(IPrimitiveWriter writer, in float value) => writer.WriteValue(value);
	void IGenericPrimitive<double>.WriteValue(IPrimitiveWriter writer, in double value) => writer.WriteValue(value);
	void IGenericPrimitive<string>.WriteValue(IPrimitiveWriter writer, in string value) => writer.WriteValue(value);
	void IGenericPrimitive<byte[]>.WriteValue(IPrimitiveWriter writer, in byte[] value) => writer.WriteValue(value);
}
