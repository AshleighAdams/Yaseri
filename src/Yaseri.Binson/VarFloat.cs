using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Yaseri.Binson;

[StructLayout(LayoutKind.Explicit)]
internal struct Ieee754Binary16
{
	[FieldOffset(0)]
	public ushort Data;
	[FieldOffset(0)]
	public Half Value;

	public override string ToString() => $"{Data:X2}";

	public const int SignBits = 1;
	public const int ExponentBits = 5;
	public const int MantissaBits = 10;
	public const int TotalBits = SignBits + ExponentBits + MantissaBits;
}

[StructLayout(LayoutKind.Explicit)]
internal struct Ieee754Binary32
{
	[FieldOffset(0)]
	public uint Data;
	[FieldOffset(0)]
	public float Value;

	public override string ToString() => $"{Data:X4}";

	public const int SignBits = 1;
	public const int ExponentBits = 8;
	public const int MantissaBits = 23;
	public const int TotalBits = SignBits + ExponentBits + MantissaBits;
}

[StructLayout(LayoutKind.Explicit)]
internal struct Ieee754Binary64
{
	[FieldOffset(0)]
	public ulong Data;
	[FieldOffset(0)]
	public double Value;

	public override string ToString() => $"{Data:X8}";

	public const int SignBits = 1;
	public const int ExponentBits = 11;
	public const int MantissaBits = 52;
	public const int TotalBits = SignBits + ExponentBits + MantissaBits;
}

file static class IntegerEncoders
{
	public static ulong EncodeZigzag(this long value)
	{
		return (ulong)((value << 1) ^ (value >> 63));
	}

	public static long DecodeZigzag(this ulong value)
	{
		return ((long)value >> 1) ^ -((long)value & 1);
	}

	public static long EncodeExponent(this ulong value)
	{
		return value switch
		{
			0 => 0,
			< 1023 => (long)value - 1023,
			>= 1023 => (long)value - 1022,
		};
	}

	public static ulong DecodeExponent(this long value)
	{
		return value switch
		{
			0 => 0,
			< 0 => (ulong)value + 1023,
			> 0 => (ulong)value + 1022,
		};
	}

	public static ulong ReverseBits(this ulong value)
	{
		ulong ret = value;
		ret = ret >> 32 | ret << 32;
		ret = (ret & 0xffff0000ffff0000) >> 16 | (ret & 0x0000ffff0000ffff) << 16;
		ret = (ret & 0xff00ff00ff00ff00) >> 8 | (ret & 0x00ff00ff00ff00ff) << 8;
		ret = (ret & 0xf0f0f0f0f0f0f0f0) >> 4 | (ret & 0x0f0f0f0f0f0f0f0f) << 4;
		ret = (ret & 0xcccccccccccccccc) >> 2 | (ret & 0x3333333333333333) << 2;
		ret = (ret & 0xaaaaaaaaaaaaaaaa) >> 1 | (ret & 0x5555555555555555) << 1;
		return ret;
	}

	public static uint ReverseBits(this uint value)
	{
		uint ret = value;
		ret = ret >> 16 | ret << 16;
		ret = (ret & 0xff00ff00) >> 8 | (ret & 0x00ff00ff) << 8;
		ret = (ret & 0xf0f0f0f0) >> 4 | (ret & 0x0f0f0f0f) << 4;
		ret = (ret & 0xcccccccc) >> 2 | (ret & 0x33333333) << 2;
		ret = (ret & 0xaaaaaaaa) >> 1 | (ret & 0x55555555) << 1;
		return ret;
	}

	public static ushort ReverseBits(this ushort value)
	{
		uint ret = value;
		ret = ret >> 8 | ret << 8;
		ret = (ret & 0xf0f0) >> 4 | (ret & 0x0f0f) << 4;
		ret = (ret & 0xcccc) >> 2 | (ret & 0x3333) << 2;
		ret = (ret & 0xaaaa) >> 1 | (ret & 0x5555) << 1;
		return (ushort)ret;
	}
}

public static class VarFloat
{
	// but for reading, the encoder could be faulty or malicious, so be conservative
	public const int MaxBufferLength = VarInt.MaxBufferLength;

	internal static int ReadFloat16Data(ReadOnlySpan<byte> buffer, ref Ieee754Binary16 value)
	{
		int readBytes = VarInt.ReadPositiveInteger(buffer, out var combinedInt);
		if (readBytes == -1)
			return -1;

		value.Data = ((ushort)(combinedInt << 1)).ReverseBits();
		return readBytes;
	}

	internal static int ReadFloat32Data(ReadOnlySpan<byte> buffer, ref Ieee754Binary32 value)
	{
		int readBytes = VarInt.ReadPositiveInteger(buffer, out var combinedInt);
		if (readBytes == -1)
			return -1;

		value.Data = ((uint)(combinedInt << 1)).ReverseBits();
		return readBytes;
	}

	internal static int ReadFloat64Data(ReadOnlySpan<byte> buffer, ref Ieee754Binary64 value)
	{
		int readBytes = VarInt.ReadPositiveInteger(buffer, out var combinedInt);
		if (readBytes == -1)
			return -1;

		value.Data = (combinedInt << 1).ReverseBits();
		return readBytes;
	}

	internal static int WriteFloat16Data(ref Ieee754Binary16 value, Span<byte> buffer)
	{
		var data = (ushort)(value.Data.ReverseBits() >> 1);
		int wroteCombined = VarInt.WritePositiveInteger(data, buffer);
		return wroteCombined;
	}

	internal static int WriteFloat32Data(ref Ieee754Binary32 value, Span<byte> buffer)
	{
		var data = value.Data.ReverseBits() >> 1;
		int wroteCombined = VarInt.WritePositiveInteger(data, buffer);
		return wroteCombined;
	}

	internal static int WriteFloat64Data(ref Ieee754Binary64 value, Span<byte> buffer)
	{
		// reverse the bits to move the nearly always not zero) exponent
		var data = value.Data.ReverseBits() >> 1;
		int wroteCombined = VarInt.WritePositiveInteger(data, buffer);
		return wroteCombined;
	}

	public static int ReadPositiveFloat16(ReadOnlySpan<byte> buffer, out Half value)
	{
		Ieee754Binary16 bin16 = default;
		var ret = ReadFloat16Data(buffer, ref bin16);
		value = bin16.Value;
		return ret;
	}
	public static int ReadPositiveFloat32(ReadOnlySpan<byte> buffer, out float value)
	{
		Ieee754Binary32 bin32 = default;
		var ret = ReadFloat32Data(buffer, ref bin32);
		value = bin32.Value;
		return ret;
	}

	public static int ReadPositiveFloat64(ReadOnlySpan<byte> buffer, out double value)
	{
		Ieee754Binary64 bin64 = default;
		var ret = ReadFloat64Data(buffer, ref bin64);
		value = bin64.Value;
		return ret;
	}

	public static int WritePositiveFloat16(Half value, Span<byte> buffer)
	{
		Ieee754Binary16 bin16 = default;
		bin16.Value = value;
		return WriteFloat16Data(ref bin16, buffer);
	}

	public static int WritePositiveFloat32(float value, Span<byte> buffer)
	{
		Ieee754Binary32 bin32 = default;
		bin32.Value = value;
		return WriteFloat32Data(ref bin32, buffer);
	}

	public static int WritePositiveFloat64(double value, Span<byte> buffer)
	{
		Ieee754Binary64 bin64 = default;
		bin64.Value = value;
		return WriteFloat64Data(ref bin64, buffer);
	}
}
