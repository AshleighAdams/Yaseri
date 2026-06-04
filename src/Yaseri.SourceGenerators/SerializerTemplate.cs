using System.Collections.Generic;
using System.Text;

namespace Yaseri.SourceGenerators;

public record struct PropertyMetadata
{
	public string FullType { get; set; }
	public string PropertyName { get; set; }
	public string Key { get; set; }
	public bool Obsolete { get; set; }
}

public static class SerializerTemplate
{
	public static string ToLiteral(this string input)
	{
		var literal = new StringBuilder(input.Length + 2);
		foreach (var c in input)
		{
			switch (c)
			{
				case '\"':
					literal.Append("\\\"");
					break;
				case '\\':
					literal.Append(@"\\");
					break;
				case '\0':
					literal.Append(@"\0");
					break;
				case '\a':
					literal.Append(@"\a");
					break;
				case '\b':
					literal.Append(@"\b");
					break;
				case '\f':
					literal.Append(@"\f");
					break;
				case '\n':
					literal.Append(@"\n");
					break;
				case '\r':
					literal.Append(@"\r");
					break;
				case '\t':
					literal.Append(@"\t");
					break;
				case '\v':
					literal.Append(@"\v");
					break;
				default:
					// ASCII printable character
					if (c >= 0x20 && c <= 0x7e)
					{
						literal.Append(c);
						// As UTF16 escaped character
					}
					else
					{
						literal.Append(@"\u");
						literal.Append(((int)c).ToString("x4"));
					}
					break;
			}
		}
		return literal.ToString();
	}

	public static void Generate(
		this StringBuilder sb,
		string visibility,
		string? declaringNamespace,
		string? baseClass,
		string fullTypeName,
		string typeName,
		IEnumerable<PropertyMetadata> properties)
	{
		var readers = new StringBuilder();
		var writers = new StringBuilder();
		var hints = new StringBuilder();

		var hintTypeName = fullTypeName.Substring("global::".Length);

		foreach (var prop in properties)
		{
			var localName = "@_" + prop.PropertyName.ToLowerInvariant();

			readers.AppendLine(
				$$"""
						if (global::System.MemoryExtensions.SequenceEqual(key, "{{prop.Key.ToLiteral()}}"u8))
						{
							if (!reader.TryReadValue(out {{prop.FullType}} {{localName}}))
							{
								value = @_YaseriContext.FailValue;
								return false;
							}
							ret.{{prop.PropertyName}} = {{localName}};
							continue;
						}
				""");

			// don't try to write out properties marked as obsolete
			if (prop.Obsolete)
				continue;

			writers.AppendLine(
				$$"""
						if (writeDefault || value.{{prop.PropertyName}} != @_YaseriContext.DefaultValue.{{prop.PropertyName}})
						{
							writer.WriteKey("{{prop.Key.ToLiteral()}}"u8);
							writer.WriteValue(value.{{prop.PropertyName}});
						}
				""");
		}

		sb.AppendLine(
			$$"""
			#nullable enable
			#pragma warning disable CS8600
			#pragma warning disable CS8601
			#pragma warning disable CS0618

			using Yaseri;
			using Yaseri.Ducks;

			namespace {{declaringNamespace}};

			file class _YaseriContext
			{
				public static readonly {{fullTypeName}} DefaultValue = new();
				public static readonly {{fullTypeName}} FailValue = new();
				public static PrimitiveTypeHint TypeHint = new()
				{
					TypeName = "{{hintTypeName.ToLiteral()}}",
					Type = typeof({{fullTypeName}}),
				};

				{{hints}}
			}

			{{visibility}} partial class {{typeName}} : global::Yaseri.IPrimitive<{{typeName}}>
			{
				public static bool TryReadValue(
					global::Yaseri.IPrimitiveReader reader,
					out {{fullTypeName}} value)
				{
					reader.ValueHint(@_YaseriContext.TypeHint);
					if (!reader.TryReadStartObject())
					{
						reader.LastError = "{{typeName}} expected start object";
						value = @_YaseriContext.FailValue;
						return false;
					}

					var ret = new {{fullTypeName}}();
					while (!reader.TryReadEndObject())
					{
						if (!reader.TryReadKey(out var keyMem))
						{
							reader.LastError = "Expected key";
							value = @_YaseriContext.FailValue;
							return false;
						}
						var key = keyMem.Span;

						{{readers}}
					}
					value = ret;
					return true;
				}

				bool global::Yaseri.IPrimitive<{{typeName}}>.InstancedTryReadValue(
					global::Yaseri.IPrimitiveReader reader,
					out {{fullTypeName}} value)
				{
					return TryReadValue(reader, out value);
				}
			
				public static void WriteValue(
					global::Yaseri.IPrimitiveWriter writer,
					in {{fullTypeName}} value)
				{
					writer.ValueHint(@_YaseriContext.TypeHint);
					bool writeDefault = writer.ShouldWriteDefaultValues;
					writer.WriteStartObject();

					{{writers}}

					writer.WriteEndObject();
				}

				void global::Yaseri.IPrimitive<{{typeName}}>.InstancedWriteValue(
					global::Yaseri.IPrimitiveWriter writer,
					in {{fullTypeName}} value)
				{
					WriteValue(writer, value);
				}
			}
			""");
	}
}
