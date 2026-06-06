#define WRITE_FILES

using System;
using System.Collections.Generic;

using FluentAssertions;

using Xunit;

using Yaseri.Attributes;
using Yaseri.Binson;
using Yaseri.Json;

namespace UnitTests;

[YaseriSerializable]
internal sealed partial class RoundTripTestObject
{
	public float Percent { get; set; } = 0.5f;

	public string Cool { get; set; } = "MyFile.png";

	[Format("text/json")]
	public string SomeJson { get; set; } = "{null, true}";

	public bool? MaybeBoolA { get; set; } = null;
	public bool? MaybeBoolB { get; set; } = null;

	public IReadOnlyList<ArrayItem> CustomArray { get; set; } = Array.Empty<ArrayItem>();
}

public class BinsonRoundTripTests
{
	[Fact]
	public void JsonToBinsonToJsonRoundTrips()
	{
		var src = new RoundTripTestObject()
		{
			MaybeBoolA = true,
			MaybeBoolB = null,
			CustomArray = new ArrayItem[]
			{
				new(){ Thing = 1337, OtherThing = 1 },
				new(){ Thing = 0, OtherThing = -420 },
			},
		};

		var binsonWriter = new BinsonPrimitiveWriter
		{
			ShouldWriteDefaultValues = true
		};

		RoundTripTestObject.WriteValue(binsonWriter, src);

		var data = binsonWriter.GetContent();
		var binsonReader = new BinsonPrimitiveReader(data, "test.binson");

#if WRITE_FILES
		var jsonWriter = new JsonPrimitiveWriter
		{
			ShouldWriteDefaultValues = true
		};
		RoundTripTestObject.WriteValue(jsonWriter, src);

		System.IO.File.WriteAllBytes("test.binson", data.ToArray());
		System.IO.File.WriteAllBytes("test.json", jsonWriter.GetJson().ToArray());
#endif

		bool read = RoundTripTestObject.TryReadValue(binsonReader, out var dst);

		read.Should().BeTrue();

		dst.Should().BeEquivalentTo(src);
	}
}
