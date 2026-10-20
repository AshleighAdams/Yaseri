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
	[Constraints(Minimum = 0.0f, Maximum = 1.0f, Step = 0.01f)]
	[PropertyName("perc")]
	public float Percent { get; set; } = 0.5f;

	[Usage("Texture")]
	public string Cool { get; set; } = "MyFile.png";

	[Format("text/json")]
	public string SomeJson { get; set; } = "{null, true}";

	public int? MaybeInt { get; set; } = null;

	public IReadOnlyList<ArrayItem> CustomArray { get; set; } = Array.Empty<ArrayItem>();

	public IReadOnlyDictionary<string, DictItem> CustomDictionary { get; set; } = new SortedDictionary<string, DictItem>();
}

[YaseriSerializable(WriteInline = true)]
internal sealed partial class ArrayItem
{
	public int Thing { get; set; }
	public int OtherThing { get; set; }
}

[YaseriSerializable(WriteInline = true)]
internal sealed partial class DictItem
{
	public string Test { get; set; } = string.Empty;
}


[YaseriSerializable(SkipUnknown = true)]
internal sealed partial class ClassWithSkipUnknown
{
	public string Test { get; set; } = string.Empty;
}

[YaseriSerializable]
internal sealed partial class ClassWithoutSkipUnknown
{
	public string Test { get; set; } = string.Empty;
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
			CustomDictionary = new SortedDictionary<string, DictItem>()
			{
				["first"] = new DictItem() { Test = "1st" },
				["second"] = new DictItem() { Test = "2nd" },
			}
		};

		writer.WriteValue(testObj);

		var json = writer.ToString();
		var expected =
			"""
			{
				"perc": 0.5,
				"Cool": "MyFile.png",
				"SomeJson": "{null, true}",
				"MaybeInt": null,
				"CustomArray": [
					{"Thing": 1, "OtherThing": 2},
					{"Thing": 3, "OtherThing": 4},
				],
				"CustomDictionary": {
					"first": {"Test": "1st"},
					"second": {"Test": "2nd"},
				},
			}
			""";

		json.Should().Be(expected);
	}

	[Fact]
	public void UnknownKeysCanBeSkipped()
	{
		var testJson =
			"""
			{
				"Test": "Hello",
				"Unknown": "World",
			}
			"""u8;

		IPrimitiveReader reader = new JsonPrimitiveReader(testJson.ToArray(), "test.json");
		reader.TryReadValue(out ClassWithSkipUnknown value).Should().BeTrue(reader.LastError);
		value.Test.Should().Be("Hello");

		reader = new JsonPrimitiveReader(testJson.ToArray(), "test.json");
		reader.TryReadValue(out ClassWithoutSkipUnknown otherValue).Should().BeFalse();
		reader.LastError.Should().Be("ClassWithoutSkipUnknown: Unknown key: Unknown");
	}
}
