using System;
using System.Numerics;

namespace Yaseri.Binson;

public static class VarInt
{
	/*
	7bits  = 0xxxxxxx
	14bits = 10xxxxxx xxxxxxxx
	21bits = 110xxxxx xxxxxxxx xxxxxxxx
	28bits = 1110xxxx xxxxxxxx xxxxxxxx xxxxxxxx
	35bits = 11110xxx xxxxxxxx xxxxxxxx xxxxxxxx xxxxxxxx
	42bits = 111110xx xxxxxxxx xxxxxxxx xxxxxxxx xxxxxxxx xxxxxxxx
	49bits = 1111110x xxxxxxxx xxxxxxxx xxxxxxxx xxxxxxxx xxxxxxxx xxxxxxxx
	56bits = 11111110 xxxxxxxx xxxxxxxx xxxxxxxx xxxxxxxx xxxxxxxx xxxxxxxx xxxxxxxx
	63bits = 11111111 0xxxxxxx xxxxxxxx xxxxxxxx xxxxxxxx xxxxxxxx xxxxxxxx xxxxxxxx xxxxxxxx
	70bits = 11111111 10xxxxxx xxxxxxxx xxxxxxxx xxxxxxxx xxxxxxxx xxxxxxxx xxxxxxxx xxxxxxxx xxxxxxxx
	*/
	private const ulong TwoPow7 = 1ul << 7;
	private const ulong TwoPow14 = 1ul << 14;
	private const ulong TwoPow21 = 1ul << 21;
	private const ulong TwoPow28 = 1ul << 28;
	private const ulong TwoPow35 = 1ul << 35;
	private const ulong TwoPow42 = 1ul << 42;
	private const ulong TwoPow49 = 1ul << 49;
	private const ulong TwoPow56 = 1ul << 56;
	private const ulong TwoPow63 = 1ul << 63;
	//private const ulong TwoPow70 = 1180591620717411303424;


	private const ulong MaxFor1Byte = TwoPow7;
	private const ulong MaxFor2Bytes = TwoPow14;
	private const ulong MaxFor3Bytes = TwoPow21;
	private const ulong MaxFor4Bytes = TwoPow28;
	private const ulong MaxFor5Bytes = TwoPow35;
	private const ulong MaxFor6Bytes = TwoPow42;
	private const ulong MaxFor7Bytes = TwoPow49;
	private const ulong MaxFor8Bytes = TwoPow56;
	private const ulong MaxFor9Bytes = TwoPow63;
	//private const ulong MaxFor10Bytes = TwoPow70;
	public const int MaxBufferLength = 10;

	public static int ReadPositiveInteger(ReadOnlySpan<byte> buffer, out ulong value)
	{
		var firstByte = buffer[0];
		var extraBytes = BitOperations.LeadingZeroCount(0b00000000000000000000000011111111 & ~(uint)firstByte) - 24;

		switch (extraBytes)
		{
			case 0:
				value = (firstByte & 0b01111111ul) << 0;
				return 1;
			case 1:
				value =
					(firstByte & 0b00111111ul) << 8 |
					((ulong)buffer[1] << 0);
				return 2;
			case 2:
				value =
					(firstByte & 0b00011111ul) << 16 |
					((ulong)buffer[1] << 8) |
					((ulong)buffer[2] << 0);
				return 3;
			case 3:
				value =
					(firstByte & 0b00001111ul) << 24 |
					((ulong)buffer[1] << 16) |
					((ulong)buffer[2] << 8) |
					((ulong)buffer[3] << 0);
				return 4;
			case 4:
				value =
					(firstByte & 0b00000111ul) << 32 |
					((ulong)buffer[1] << 24) |
					((ulong)buffer[2] << 16) |
					((ulong)buffer[3] << 8) |
					((ulong)buffer[4] << 0);
				return 5;
			case 5:
				value =
					(firstByte & 0b00000011ul) << 40 |
					((ulong)buffer[1] << 32) |
					((ulong)buffer[2] << 24) |
					((ulong)buffer[3] << 16) |
					((ulong)buffer[4] << 8) |
					((ulong)buffer[5] << 0);
				return 6;
			case 6:
				value =
					(firstByte & 0b00000001ul) << 48 |
					((ulong)buffer[1] << 40) |
					((ulong)buffer[2] << 32) |
					((ulong)buffer[3] << 24) |
					((ulong)buffer[4] << 16) |
					((ulong)buffer[5] << 8) |
					((ulong)buffer[6] << 0);
				return 7;
			case 7:
				value =
					((ulong)buffer[1] << 48) |
					((ulong)buffer[2] << 40) |
					((ulong)buffer[3] << 32) |
					((ulong)buffer[4] << 24) |
					((ulong)buffer[5] << 16) |
					((ulong)buffer[6] << 8) |
					((ulong)buffer[7] << 0);
				return 8;
			default:
				break;
		}

		var secondByte = buffer[1];
		extraBytes = BitOperations.LeadingZeroCount(0b00000000000000000000000011111111 & ~(uint)secondByte) - 24;

		switch (extraBytes)
		{
			case 0:
				value =
					(secondByte & 0b01111111ul) << 56 |
					((ulong)buffer[2] << 48) |
					((ulong)buffer[3] << 40) |
					((ulong)buffer[4] << 32) |
					((ulong)buffer[5] << 24) |
					((ulong)buffer[6] << 16) |
					((ulong)buffer[7] << 8) |
					((ulong)buffer[8] << 0);
				return 9;
			case 1:
				// if any bits are set here, it's too high...
				if ((secondByte & 0b00111111ul) != 0)
				{
					value = 0;
					return -1;
				}
				value =
					((ulong)buffer[2] << 56) |
					((ulong)buffer[3] << 48) |
					((ulong)buffer[4] << 40) |
					((ulong)buffer[5] << 32) |
					((ulong)buffer[6] << 24) |
					((ulong)buffer[7] << 16) |
					((ulong)buffer[8] << 8) |
					((ulong)buffer[9] << 0);
				return 10;
			default:
				value = 0;
				return -1;
		}
	}

	public static int WritePositiveInteger(ulong value, Span<byte> buffer)
	{
		if (buffer.Length < MaxBufferLength)
			throw new ArgumentException("Buffer to small", nameof(buffer));

		switch (value)
		{
			case < MaxFor1Byte:
				buffer[0] = (byte)value;
				return 1;
			case < MaxFor2Bytes:
				buffer[0] = (byte)(0b10_000000 | (0b00_111111 & (value >> 8)));
				buffer[1] = (byte)(0b_00000000 | (0b_11111111 & (value >> 0)));
				return 2;
			case < MaxFor3Bytes:
				buffer[0] = (byte)(0b110_00000 | (0b000_11111 & (value >> 16)));
				buffer[1] = (byte)(0b_00000000 | (0b_11111111 & (value >> 8)));
				buffer[2] = (byte)(0b_00000000 | (0b_11111111 & (value >> 0)));
				return 3;
			case < MaxFor4Bytes:
				buffer[0] = (byte)(0b1110_0000 | (0b0000_1111 & (value >> 24)));
				buffer[1] = (byte)(0b_00000000 | (0b_11111111 & (value >> 16)));
				buffer[2] = (byte)(0b_00000000 | (0b_11111111 & (value >> 8)));
				buffer[3] = (byte)(0b_00000000 | (0b_11111111 & (value >> 0)));
				return 4;
			case < MaxFor5Bytes:
				buffer[0] = (byte)(0b11110_000 | (0b00000_111 & (value >> 32)));
				buffer[1] = (byte)(0b_00000000 | (0b_11111111 & (value >> 24)));
				buffer[2] = (byte)(0b_00000000 | (0b_11111111 & (value >> 16)));
				buffer[3] = (byte)(0b_00000000 | (0b_11111111 & (value >> 8)));
				buffer[4] = (byte)(0b_00000000 | (0b_11111111 & (value >> 0)));
				return 5;
			case < MaxFor6Bytes:
				buffer[0] = (byte)(0b111110_00 | (0b000000_11 & (value >> 40)));
				buffer[1] = (byte)(0b_00000000 | (0b_11111111 & (value >> 32)));
				buffer[2] = (byte)(0b_00000000 | (0b_11111111 & (value >> 24)));
				buffer[3] = (byte)(0b_00000000 | (0b_11111111 & (value >> 16)));
				buffer[4] = (byte)(0b_00000000 | (0b_11111111 & (value >> 8)));
				buffer[5] = (byte)(0b_00000000 | (0b_11111111 & (value >> 0)));
				return 6;
			case < MaxFor7Bytes:
				buffer[0] = (byte)(0b1111110_0 | (0b0000000_1 & (value >> 48)));
				buffer[1] = (byte)(0b_00000000 | (0b_11111111 & (value >> 40)));
				buffer[2] = (byte)(0b_00000000 | (0b_11111111 & (value >> 32)));
				buffer[3] = (byte)(0b_00000000 | (0b_11111111 & (value >> 24)));
				buffer[4] = (byte)(0b_00000000 | (0b_11111111 & (value >> 16)));
				buffer[5] = (byte)(0b_00000000 | (0b_11111111 & (value >> 8)));
				buffer[6] = (byte)(0b_00000000 | (0b_11111111 & (value >> 0)));
				return 7;
			case < MaxFor8Bytes:
				buffer[0] = 0b11111110;
				buffer[1] = (byte)(0b_00000000 | (0b_11111111 & (value >> 48)));
				buffer[2] = (byte)(0b_00000000 | (0b_11111111 & (value >> 40)));
				buffer[3] = (byte)(0b_00000000 | (0b_11111111 & (value >> 32)));
				buffer[4] = (byte)(0b_00000000 | (0b_11111111 & (value >> 24)));
				buffer[5] = (byte)(0b_00000000 | (0b_11111111 & (value >> 16)));
				buffer[6] = (byte)(0b_00000000 | (0b_11111111 & (value >> 8)));
				buffer[7] = (byte)(0b_00000000 | (0b_11111111 & (value >> 0)));
				return 8;
			case < MaxFor9Bytes:
				buffer[0] = 0b11111111;
				buffer[1] = (byte)(0b0_0000000 | (0b0_1111111 & (value >> 56)));
				buffer[2] = (byte)(0b_00000000 | (0b_11111111 & (value >> 48)));
				buffer[3] = (byte)(0b_00000000 | (0b_11111111 & (value >> 40)));
				buffer[4] = (byte)(0b_00000000 | (0b_11111111 & (value >> 32)));
				buffer[5] = (byte)(0b_00000000 | (0b_11111111 & (value >> 24)));
				buffer[6] = (byte)(0b_00000000 | (0b_11111111 & (value >> 16)));
				buffer[7] = (byte)(0b_00000000 | (0b_11111111 & (value >> 8)));
				buffer[8] = (byte)(0b_00000000 | (0b_11111111 & (value >> 0)));
				return 9;
			default:
				buffer[0] = 0b11111111;
				buffer[1] = 0b10_000000; // bits on this are unused, as they would represent bits bits 65 to 70 of a 64bit integer
				buffer[2] = (byte)(0b_00000000 | (0b_11111111 & (value >> 56)));
				buffer[3] = (byte)(0b_00000000 | (0b_11111111 & (value >> 48)));
				buffer[4] = (byte)(0b_00000000 | (0b_11111111 & (value >> 40)));
				buffer[5] = (byte)(0b_00000000 | (0b_11111111 & (value >> 32)));
				buffer[6] = (byte)(0b_00000000 | (0b_11111111 & (value >> 24)));
				buffer[7] = (byte)(0b_00000000 | (0b_11111111 & (value >> 16)));
				buffer[8] = (byte)(0b_00000000 | (0b_11111111 & (value >> 8)));
				buffer[9] = (byte)(0b_00000000 | (0b_11111111 & (value >> 0)));
				return 10;
		}
	}
}

