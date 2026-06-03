using System;
using System.Diagnostics.CodeAnalysis;

using FluentAssertions;

using Yaseri.Json;

using Xunit;

namespace UnitTests;

public class JsonPrimitiveWriterTests
{
	[Fact]
	public void IndentationAndSeperatorsFunctionAsExpected()
	{
		using var writer = new JsonPrimitiveWriter();
		writer.WriteStartObject();

		writer.WriteKey("empty-array"u8);
		writer.WriteStartArray();
		writer.WriteEndArray();

		writer.WriteKey("some-nulls"u8);
		writer.WriteStartArray();
		writer.WriteNull();
		writer.WriteNull();
		writer.WriteEndArray();

		writer.WriteKey("inline-nulls"u8);
		writer.WriteStartArray(writeInline: true);
		writer.WriteNull();
		writer.WriteNull();
		writer.WriteEndArray();

		writer.WriteKey("inline-nulls-obj"u8);
		writer.WriteStartObject(writeInline: true);
		writer.WriteKey("a"u8);
		writer.WriteNull();
		writer.WriteKey("b"u8);
		writer.WriteNull();
		writer.WriteEndObject();

		writer.WriteKey("some-null"u8);
		writer.WriteNull();

		writer.WriteEndObject();

		string json = writer.ToString();
		string expected =
			"""
			{
				"empty-array": [],
				"some-nulls": [
					null,
					null,
				],
				"inline-nulls": [null, null],
				"inline-nulls-obj": {"a": null, "b": null},
				"some-null": null,
			}
			""";

		json.Should().Be(expected);
	}
	[Fact]
	public void CanWriteBools()
	{
		using var writer = new JsonPrimitiveWriter();
		writer.WriteStartObject();

		writer.WriteKey("true"u8);
		writer.WriteValue(true);
		writer.WriteKey("false"u8);
		writer.WriteValue(false);

		writer.WriteEndObject();

		string json = writer.ToString();
		string expected =
			"""
			{
				"true": true,
				"false": false,
			}
			""";

		json.Should().Be(expected);
	}

	[Fact]
	public void CanWriteNumbers()
	{
		using var writer = new JsonPrimitiveWriter();
		writer.WriteStartObject();

		writer.WriteKey("byte"u8);
		writer.WriteValue((byte)255);

		writer.WriteKey("uint32"u8);
		writer.WriteValue(42069);

		writer.WriteKey("int32"u8);
		writer.WriteValue(-42069);

		writer.WriteKey("float32"u8);
		writer.WriteValue(13.37f);

		writer.WriteKey("float64"u8);
		writer.WriteValue(Math.PI);

		writer.WriteEndObject();

		string json = writer.ToString();
		string expected =
			"""
			{
				"byte": 255,
				"uint32": 42069,
				"int32": -42069,
				"float32": 13.37,
				"float64": 3.141592653589793,
			}
			""";

		json.Should().Be(expected);
	}

	[Fact]
	public void CanWriteStrings()
	{
		using var writer = new JsonPrimitiveWriter();
		writer.WriteStartObject();

		writer.WriteKey("empty-str"u8);
		writer.WriteValue("");

		writer.WriteKey("simple-str"u8);
		writer.WriteValue("hello");

		writer.WriteEndObject();

		string json = writer.ToString();
		string expected =
			"""
			{
				"empty-str": "",
				"simple-str": "hello",
			}
			""";

		json.Should().Be(expected);
	}
}
