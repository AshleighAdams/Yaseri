using FluentAssertions;

using Xunit;

using Yaseri.Binson;

using System.Linq;

namespace UnitTests;

public class BinsonVarIntTests
{
	[Theory]
	[InlineData(0,   new byte[] { 0b00000000 })]
	[InlineData(1,   new byte[] { 0b00000001 })]
	[InlineData(2,   new byte[] { 0b00000010 })]
	[InlineData(127, new byte[] { 0b01111111 })]
	[InlineData(128, new byte[] { 0b10000000, 0b10000000 })]
	[InlineData(255, new byte[] { 0b10000000, 0b11111111 })]
	[InlineData(256, new byte[] { 0b10000001, 0b00000000 })]
	[InlineData(ulong.MaxValue, new byte[] { 0b11111111, 0b10000000, 0b11111111, 0b11111111, 0b11111111, 0b11111111, 0b11111111, 0b11111111, 0b11111111, 0b11111111 })]
	public void VarIntEncodesAndDecodesCorrectly(ulong value, byte[] expected)
	{
		var buffer = new byte[VarInt.MaxBufferLength];
		var bytesWritten = VarInt.WritePositiveInteger(value, buffer);
		var valueWritten = buffer[..bytesWritten].ToArray();
		valueWritten.Should().BeEquivalentTo(expected);

		var bytesRead = VarInt.ReadPositiveInteger(buffer, out ulong readValue);
		bytesRead.Should().Be(expected.Length);
		readValue.Should().Be(value);
	}
}
