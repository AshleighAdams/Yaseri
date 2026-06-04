using System;
using System.Collections.Generic;
using System.Text;

using FluentAssertions;

using Yaseri.Json;

using Xunit;

namespace UnitTests;

public class JsonPrimitiveSerializerTests
{
	private readonly Encoding Utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
	private record struct ExpectedJsonToken(JsonTokenType Type, string? Literal = null)
	{
		public static bool operator ==(JsonToken left, ExpectedJsonToken right)
		{
			if (left.Type != right.Type)
				return false;
			if (right.Literal is not null)
			{
				if (right.Type == JsonTokenType.String)
				{
					var literal = left.ReadString();
					if (literal != right.Literal)
						return false;
				}
				else if (left.TokenSource != right.Literal)
					return false;
			}
			return true;
		}
		public static bool operator !=(JsonToken left, ExpectedJsonToken right) => !(left.Type == right.Type);
	}

	[Fact]
	public void TokenizesProperly()
	{
		string json =
			"""
			{
				"some-null": null,
				"some-true": true,
				"some-false": false,
				"some-int": 1337,
				"some-float": 13.37,
				"some-int-exp": 42e+1,
				"some-float-exp": 1337E-2,
				"some-string": "They said \"I could've dropped my croissant!\"",
				"some-empty-array": [],
				"some-populated-array": [10, "hi"],
			}
			""";
		var expectedTokens = new List<ExpectedJsonToken>()
		{
			new(JsonTokenType.StartObject),

			new(JsonTokenType.String, "some-null"),
			new(JsonTokenType.NameSeparator),
			new(JsonTokenType.Null),
			new(JsonTokenType.ValueSeparator),

			new(JsonTokenType.String, "some-true"),
			new(JsonTokenType.NameSeparator),
			new(JsonTokenType.True),
			new(JsonTokenType.ValueSeparator),

			new(JsonTokenType.String, "some-false"),
			new(JsonTokenType.NameSeparator),
			new(JsonTokenType.False),
			new(JsonTokenType.ValueSeparator),

			new(JsonTokenType.String, "some-int"),
			new(JsonTokenType.NameSeparator),
			new(JsonTokenType.Number, "1337"),
			new(JsonTokenType.ValueSeparator),

			new(JsonTokenType.String, "some-float"),
			new(JsonTokenType.NameSeparator),
			new(JsonTokenType.Number, "13.37"),
			new(JsonTokenType.ValueSeparator),

			new(JsonTokenType.String, "some-int-exp"),
			new(JsonTokenType.NameSeparator),
			new(JsonTokenType.Number, "42e+1"),
			new(JsonTokenType.ValueSeparator),

			new(JsonTokenType.String, "some-float-exp"),
			new(JsonTokenType.NameSeparator),
			new(JsonTokenType.Number, "1337E-2"),
			new(JsonTokenType.ValueSeparator),

			new(JsonTokenType.String, "some-string"),
			new(JsonTokenType.NameSeparator),
			new(JsonTokenType.String, "They said \"I could've dropped my croissant!\""),
			new(JsonTokenType.ValueSeparator),

			new(JsonTokenType.String, "some-empty-array"),
			new(JsonTokenType.NameSeparator),
			new(JsonTokenType.StartArray),
			new(JsonTokenType.EndArray),
			new(JsonTokenType.ValueSeparator),

			new(JsonTokenType.String, "some-populated-array"),
			new(JsonTokenType.NameSeparator),
			new(JsonTokenType.StartArray),
			new(JsonTokenType.Number, "10"),
			new(JsonTokenType.ValueSeparator),
			new(JsonTokenType.String, "hi"),
			new(JsonTokenType.EndArray),
			new(JsonTokenType.ValueSeparator),

			new(JsonTokenType.EndObject),
		};

		byte[] jsonUtf8 = Utf8.GetBytes(json);
		var actualTokens = new List<JsonToken>();

		var success = JsonTokenizer.TryTokenize(jsonUtf8, actualTokens);
		success.Should().BeTrue();

		if (!success)
			return;

		int minCount = Math.Min(expectedTokens.Count, actualTokens.Count);
		for (int i = 0; i < minCount; i++)
		{
			bool tokenEquals = actualTokens[i] == expectedTokens[i];
			tokenEquals.Should().BeTrue();
			if (!tokenEquals)
				break;
		}

		actualTokens.Count.Should().Be(expectedTokens.Count);
	}
}
