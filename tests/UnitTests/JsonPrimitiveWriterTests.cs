using System;
using System.Diagnostics.CodeAnalysis;

using FluentAssertions;

using Yaseri.Json;

using Xunit;
using System.Numerics;
using Yaseri;
using System.Collections.Generic;

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

	[Fact]
	public void CanWriteVectors()
	{
		using var jsonWriter = new JsonPrimitiveWriter();
		IPrimitiveWriter writer = jsonWriter;

		writer.WriteStartObject();

		writer.WriteKey("vec2"u8);
		writer.WriteValue(new Vector2(1, 2));

		writer.WriteKey("vec3"u8);
		writer.WriteValue(new Vector3(3, 4, 5));

		writer.WriteKey("vec4"u8);
		writer.WriteValue(new Vector4(6, 7, 8, 9));

		writer.WriteEndObject();

		string json = jsonWriter.ToString();
		string expected =
			"""
			{
				"vec2": [1, 2],
				"vec3": [3, 4, 5],
				"vec4": [6, 7, 8, 9],
			}
			""";

		json.Should().Be(expected);
	}

	[Fact]
	public void CanWriteArrays()
	{
		using var jsonWriter = new JsonPrimitiveWriter();
		IPrimitiveWriter writer = jsonWriter;

		writer.WriteStartObject();

		writer.WriteKey("list"u8);
		writer.WriteValue(new List<int>() { 1, 2, 3, 4 });

		writer.WriteEndObject();

		string json = jsonWriter.ToString();
		string expected =
			"""
			{
				"list": [
					1,
					2,
					3,
					4,
				],
			}
			""";

		json.Should().Be(expected);
	}

	[Fact]
	public void EscapesKeysCorrectly()
	{
		using var jsonWriter = new JsonPrimitiveWriter();
		IPrimitiveWriter writer = jsonWriter;

		writer.WriteStartObject(writeInline: true);
		writer.WriteKey("Hello.\nWho wrote \"Hello world.\" in C:\\Hello.txt"u8);
		writer.WriteNull();
		writer.WriteEndObject();

		string json = jsonWriter.ToString();
		string expected =
			"""
			{"Hello.\nWho wrote \"Hello world.\" in C:\\Hello.txt": null}
			""";

		json.Should().Be(expected);
	}

	[Fact]
	public void EscapesValuesCorrectly()
	{
		using var jsonWriter = new JsonPrimitiveWriter();
		IPrimitiveWriter writer = jsonWriter;

		writer.WriteValue("Hello.\nWho wrote \"Hello world.\" in C:\\Hello.txt");

		string json = jsonWriter.ToString();
		string expected =
			"""
			"Hello.\nWho wrote \"Hello world.\" in C:\\Hello.txt"
			""";

		json.Should().Be(expected);
	}
}
