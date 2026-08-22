using FluentAssertions;

using Xunit;

using Yaseri.Binson;

using System.Linq;
using System;

namespace UnitTests;

public class BinsonVarFloatTests
{
	[Theory]
	[InlineData(13.37)]
	[InlineData(13.5)]
	[InlineData(0.0)]
	[InlineData(1.0)]
	public void VarFloat64EncodesAndDecodesCorrectly(double value)
	{
		var buffer = new byte[VarFloat.MaxBufferLength];
		var bytesWritten = VarFloat.WritePositiveFloat64(value, buffer);
		bytesWritten.Should().BeGreaterThan(0);
		var valueWritten = buffer[..bytesWritten].ToArray();

		var bytesRead = VarFloat.ReadPositiveFloat64(buffer, out double readValue);
		bytesRead.Should().Be(bytesWritten);
		readValue.Should().Be(value);
	}

	[Theory]
	[InlineData(13.37f)]
	[InlineData(13.5f)]
	[InlineData(0.0f)]
	[InlineData(1.0f)]
	public void VarFloat32EncodesAndDecodesCorrectly(float value)
	{
		var buffer = new byte[VarFloat.MaxBufferLength];
		var bytesWritten = VarFloat.WritePositiveFloat32(value, buffer);
		bytesWritten.Should().BeGreaterThan(0);
		var valueWritten = buffer[..bytesWritten].ToArray();

		var bytesRead = VarFloat.ReadPositiveFloat32(buffer, out float readValue);
		bytesRead.Should().Be(bytesWritten);
		readValue.Should().Be(value);
	}

	[Theory]
	[InlineData(13.37f)]
	[InlineData(13.5f)]
	[InlineData(0.0f)]
	[InlineData(1.0f)]
	public void VarFloat16EncodesAndDecodesCorrectly(float value32)
	{
		var value = (Half)value32;
		var buffer = new byte[VarFloat.MaxBufferLength];
		var bytesWritten = VarFloat.WritePositiveFloat16(value, buffer);
		bytesWritten.Should().BeGreaterThan(0);
		var valueWritten = buffer[..bytesWritten].ToArray();

		var bytesRead = VarFloat.ReadPositiveFloat16(buffer, out Half readValue);
		bytesRead.Should().Be(bytesWritten);
		readValue.Should().Be(value);
	}
}
