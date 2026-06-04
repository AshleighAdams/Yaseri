using System;
using System.Collections.Generic;

using FluentAssertions;

using Xunit;

using Yaseri;
using Yaseri.Attributes;
using Yaseri.Json;

namespace UnitTests;

[YaseriSerializable]
internal sealed partial class TestObject
{
	[Constraints(Minimum = 0.0f, Maximum = 1.0f)]
	[PropertyName("perc")]
	public float Percent { get; set; } = 0.5f;

	public bool Cool { get; set; } = false;

	public IReadOnlyList<ArrayItem> CustomArray { get; set; } = Array.Empty<ArrayItem>();
}

[YaseriSerializable]
internal sealed partial class ArrayItem
{
	public int Thing { get; set; }
	public int OtherThing { get; set; }
}

public class SourceGeneratorTests
{
	[Fact]
	public void SourceGeneratedWritesCorrectly()
	{
		var jsonWriter = new JsonPrimitiveWriter
		{
			ShouldWriteDefaultValues = true
		};

		var writer = jsonWriter as IPrimitiveWriter;

		var testObj = new TestObject()
		{
			CustomArray = new ArrayItem[]
			{
				new(){ Thing = 1, OtherThing = 2 },
				new(){ Thing = 3, OtherThing = 4 },
			},
		};

		writer.WriteValue(testObj);

		var json = writer.ToString();
		var expected =
			"""
			{
				"perc": 0.5,
				"Cool": false,
				"CustomArray": [
					{
						"Thing": 1,
						"OtherThing": 2,
					},
					{
						"Thing": 3,
						"OtherThing": 4,
					},
				]
			}
			""";

		json.Should().Be(expected);
	}
}
