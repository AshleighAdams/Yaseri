using System;
using System.Numerics;

using FluentAssertions;

using Yaseri;
using Yaseri.Json;

using Xunit;

namespace UnitTests;

public class JsonPrimitiveReaderTests
{
	[Fact]
	public void CanReadFloat32()
	{
		var json =
			"""
			1337.42
			"""u8.ToArray();

		var reader = new JsonPrimitiveReader(json, "test.json");
		reader.TryReadValue(out float value).Should().BeTrue();
		value.Should().Be(1337.42f);
	}

	[Fact]
	public void CanReadFloat64()
	{
		var json =
			"""
			3.141592653589793238462
			"""u8.ToArray();

		var reader = new JsonPrimitiveReader(json, "test.json");
		reader.TryReadValue(out double value).Should().BeTrue();
		value.Should().BeApproximately(3.141592653589793238462, double.Epsilon);
	}

	[Fact]
	public void CanReadPositiveInt32()
	{
		var json =
			"""
			2147483647
			"""u8.ToArray();

		var reader = new JsonPrimitiveReader(json, "test.json");
		reader.TryReadValue(out int value).Should().BeTrue();
		value.Should().Be(2147483647);
	}

	[Fact]
	public void CanReadNegativeInt32()
	{
		var json =
			"""
			-2147483648
			"""u8.ToArray();

		var reader = new JsonPrimitiveReader(json, "test.json");
		reader.TryReadValue(out int value).Should().BeTrue();
		value.Should().Be(-2147483648);
	}

	[Fact]
	public void CanReadPositiveInt64()
	{
		var json =
			"""
			9223372036854775807
			"""u8.ToArray();

		var reader = new JsonPrimitiveReader(json, "test.json");
		reader.TryReadValue(out long value).Should().BeTrue();
		value.Should().Be(9223372036854775807);
	}

	[Fact]
	public void CanReadNegativeInt64()
	{
		var json =
			"""
			-9223372036854775808
			"""u8.ToArray();

		var reader = new JsonPrimitiveReader(json, "test.json");
		reader.TryReadValue(out long value).Should().BeTrue();
		value.Should().Be(-9223372036854775808);
	}

	[Fact]
	public void CanReadPositiveUInt32()
	{
		var json =
			"""
			4294967295
			"""u8.ToArray();

		var reader = new JsonPrimitiveReader(json, "test.json");
		reader.TryReadValue(out ulong value).Should().BeTrue();
		value.Should().Be(4294967295);
	}

	[Fact]
	public void CanReadPositiveUInt64()
	{
		var json =
			"""
			18446744073709551615
			"""u8.ToArray();

		var reader = new JsonPrimitiveReader(json, "test.json");
		reader.TryReadValue(out ulong value).Should().BeTrue();
		value.Should().Be(18446744073709551615);
	}

	[Fact]
	public void HandlesOverflowsGracefully()
	{
		var json =
			"""
			18446744073709551616
			"""u8.ToArray();

		var reader = new JsonPrimitiveReader(json, "test.json");
		reader.TryReadValue(out ulong value).Should().BeFalse();
	}

	[Fact]
	public void CanReadArrays()
	{
		var json =
			"""
			[
				10,
				20,
				30
			]
			"""u8.ToArray();

		var reader = new JsonPrimitiveReader(json, "test.json");
		reader.TryReadStartArray().Should().BeTrue();
		reader.TryReadValue(out int x).Should().BeTrue();
		reader.TryReadValue(out int y).Should().BeTrue();
		reader.TryReadValue(out int z).Should().BeTrue();
		reader.TryReadEndArray().Should().BeTrue();

		x.Should().Be(10);
		y.Should().Be(20);
		z.Should().Be(30);
	}

	[Fact]
	public void CanReadNestedArrays()
	{
		var json =
			"""
			[
				10,
				[
					20,
					30
				],
				40
			]
			"""u8.ToArray();

		var reader = new JsonPrimitiveReader(json, "test.json");
		reader.TryReadStartArray().Should().BeTrue();
		reader.TryReadValue(out int x).Should().BeTrue();
		reader.TryReadStartArray().Should().BeTrue();
		reader.TryReadValue(out int y).Should().BeTrue();
		reader.TryReadValue(out int z).Should().BeTrue();
		reader.TryReadEndArray().Should().BeTrue();
		reader.TryReadValue(out int w).Should().BeTrue();
		reader.TryReadEndArray().Should().BeTrue();

		x.Should().Be(10);
		y.Should().Be(20);
		z.Should().Be(30);
		w.Should().Be(40);
	}

	[Fact]
	public void CanReadArrayInObject()
	{
		var json =
			"""
			{
				"my-array": [
					10,
					20,
					30
				]
			}
			"""u8.ToArray();

		var reader = new JsonPrimitiveReader(json, "test.json");
		reader.TryReadStartObject().Should().BeTrue();
		reader.TryReadKey(out ReadOnlyMemory<byte> x).Should().BeTrue();
		x.Span.SequenceEqual("my-array"u8).Should().BeTrue();
		reader.TryReadStartArray().Should().BeTrue();
		reader.TryReadValue(out int a).Should().BeTrue();
		reader.TryReadValue(out int b).Should().BeTrue();
		reader.TryReadValue(out int c).Should().BeTrue();
		reader.TryReadEndArray().Should().BeTrue();
		reader.TryReadEndObject().Should().BeTrue();

		a.Should().Be(10);
		b.Should().Be(20);
		c.Should().Be(30);
	}

	[Fact]
	public void CanReadObjectInArray()
	{
		var json =
			"""
			[
				{
					"my-array": [
						10,
						20,
						30
					],
					"my-str": "hello",
				}
			]
			"""u8.ToArray();

		var reader = new JsonPrimitiveReader(json, "test.json");
		reader.TryReadStartArray().Should().BeTrue();
		reader.TryReadStartObject().Should().BeTrue();
		reader.TryReadKey(out ReadOnlyMemory<byte> x).Should().BeTrue();
		x.Span.SequenceEqual("my-array"u8).Should().BeTrue();
		reader.TryReadStartArray().Should().BeTrue();
		reader.TryReadValue(out int a).Should().BeTrue();
		reader.TryReadValue(out int b).Should().BeTrue();
		reader.TryReadValue(out int c).Should().BeTrue();
		reader.TryReadEndArray().Should().BeTrue();
		reader.TryReadKey(out ReadOnlyMemory<byte> y).Should().BeTrue();
		y.Span.SequenceEqual("my-str"u8).Should().BeTrue();
		reader.TryReadValue(out string d).Should().BeTrue();
		reader.TryReadEndObject().Should().BeTrue();
		reader.TryReadEndArray().Should().BeTrue();

		a.Should().Be(10);
		b.Should().Be(20);
		c.Should().Be(30);
		d.Should().Be("hello");
	}

	[Fact]
	public void CanSkipToEndOfObject()
	{
		var json =
			"""
			{
				"my-object": {
					"some-array": [1, 2, 3],
					"some-other-obj": {},
					"another-obj": {
						"some-str": "here"
					}
				},
				"my-int": 1337,
			}
			"""u8.ToArray();

		var reader = new JsonPrimitiveReader(json, "test.json");
		reader.TryReadStartObject().Should().BeTrue();

		reader.TryReadKey(out ReadOnlyMemory<byte> x).Should().BeTrue();
		x.Span.SequenceEqual("my-object"u8).Should().BeTrue();
		reader.TryReadStartObject().Should().BeTrue();
		reader.TryReadEndObject(skipToEnd: true).Should().BeTrue();

		reader.TryReadKey(out ReadOnlyMemory<byte> y).Should().BeTrue();
		y.Span.SequenceEqual("my-int"u8).Should().BeTrue();
		reader.TryReadValue(out int a).Should().BeTrue();

		reader.TryReadEndObject(skipToEnd: false).Should().BeTrue();

		a.Should().Be(1337);
	}

	[Fact]
	public void CanSkipValue()
	{
		var json =
			"""
			{
				"my-object": {
					"some-array": [1, 2, 3],
					"some-other-obj": {},
					"another-obj": {
						"some-str": "here"
					}
				},
				"my-int": 1337,
			}
			"""u8.ToArray();

		var reader = new JsonPrimitiveReader(json, "test.json");
		reader.TryReadStartObject().Should().BeTrue();

		reader.TryReadKey(out ReadOnlyMemory<byte> x).Should().BeTrue();
		x.Span.SequenceEqual("my-object"u8).Should().BeTrue();
		reader.TrySkipValue().Should().BeTrue();

		reader.TryReadKey(out ReadOnlyMemory<byte> y).Should().BeTrue();
		y.Span.SequenceEqual("my-int"u8).Should().BeTrue();
		reader.TryReadValue(out int a).Should().BeTrue();

		reader.TryReadEndObject(skipToEnd: false).Should().BeTrue();

		a.Should().Be(1337);
	}

	[Fact]
	public void CanReadVector2()
	{
		var json =
			"""
			{
				"@position": [42, 69],
			}
			"""u8.ToArray();

		var reader = new JsonPrimitiveReader(json, "test.json") as IPrimitiveReader;
		reader.TryReadStartObject().Should().BeTrue();
		reader.TryReadKey("@position"u8).Should().BeTrue();
		reader.TryReadValue(out Vector2 position).Should().BeTrue();
		position.Should().Be(new Vector2(42.0f, 69.0f));
		reader.TryReadEndObject().Should().BeTrue();
	}

	[Fact]
	public void HandlesSingleLineCorrectly()
	{
		var json =
			"""
			// check comments at the begining of a file
			[
				// 1234, check single line comments are disabled
				1337
			]
			// check comments at the end of a file
			"""u8.ToArray();

		var reader = new JsonPrimitiveReader(json, "test.json") as IPrimitiveReader;
		reader.TryReadStartArray().Should().BeTrue();
		reader.TryReadValue(out int val).Should().BeTrue();
		val.Should().Be(1337);
		reader.TryReadEndArray().Should().BeTrue();
	}

	[Fact]
	public void HandlesMultilineLineCorrectly()
	{
		var json =
			"""
			/* check comments at the begining of a file /*
			[
				/*
				1234, check single line comments are disabled
				*/
				1337
			]
			/* check comments at the end of a file */
			"""u8.ToArray();

		var reader = new JsonPrimitiveReader(json, "test.json") as IPrimitiveReader;
		reader.TryReadStartArray().Should().BeTrue();
		reader.TryReadValue(out int val).Should().BeTrue();
		val.Should().Be(1337);
		reader.TryReadEndArray().Should().BeTrue();
	}

	[Fact]
	public void UnfinishedMultilineCommentHandledGracefully()
	{
		var json =
			"""
			[
				1337
			]
			/* unfinished comment
			"""u8.ToArray();

		var reader = new JsonPrimitiveReader(json, "test.json") as IPrimitiveReader;
		reader.TryReadStartArray().Should().BeTrue();
		reader.TryReadValue(out int val).Should().BeTrue();
		val.Should().Be(1337);
		reader.TryReadEndArray().Should().BeTrue();
		reader.TryReadStartObject().Should().BeFalse();
	}
}
