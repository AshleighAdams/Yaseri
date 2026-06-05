using System;
using System.Buffers.Text;
using System.IO;
using System.Text;

namespace Yaseri.Json;

public record struct JsonToken(JsonTokenType Type, Range Location, ReadOnlyMemory<byte> Source, bool NeedsUnescaping = false)
{
	public static bool operator ==(JsonToken token, ReadOnlySpan<byte> str)
	{
		if (token.Type != JsonTokenType.String)
			return false;
		if (!token.NeedsUnescaping)
			return token.Source.Span[token.Location][1..^1].SequenceEqual(str);

		// TODO: Optimize this to do the comparison in an escape-aware way, rather than interpreting the value
		// and then comparing
		return token.ReadUtf8String().Span.SequenceEqual(str);
	}
	public static bool operator !=(JsonToken token, ReadOnlySpan<byte> str) => !(token == str);

	private static readonly UTF8Encoding Utf8 = new(encoderShouldEmitUTF8Identifier: false);
	public readonly ReadOnlySpan<byte> TokenSourceUtf8 => Source.Span[Location];
	public readonly string TokenSource => Utf8.GetString(TokenSourceUtf8);

	public readonly string ReadString()
	{
		if (Type != JsonTokenType.String)
			throw new InvalidOperationException("Attempted to get a string from a non-string token");

		return Utf8.GetString(ReadUtf8String().Span);
	}

	public readonly ReadOnlyMemory<byte> ReadUtf8String()
	{
		var unicodeChars = string.Empty;
		var contentMem = Source[Location][1..^1];

		if (!NeedsUnescaping)
			return contentMem;

		using var ms = new MemoryStream(contentMem.Length);
		var content = contentMem.Span;

		int i = 0;
		while (i < content.Length)
		{
			byte c = content[i];
			if (c == (byte)'"')
				throw new InvalidOperationException("Assertion failure: Unescaped quote found?");
			else if (c == (byte)'\\')
			{
				i++;
				if (i >= content.Length)
					throw new InvalidOperationException("Assertion failure: Unfinished escape sequence found?");

				c = content[i];
				i++;

				switch (c)
				{
					case (byte)'"':
					case (byte)'\\':
					case (byte)'/':
					case (byte)'\'': // NOTE: "\'" is not technically legal json, but it is commonly found in the wild
						ms.WriteByte(c);
						continue;
					case (byte)'b':
						ms.WriteByte((byte)'\b');
						continue;
					case (byte)'f':
						ms.WriteByte((byte)'\f');
						continue;
					case (byte)'n':
						ms.WriteByte((byte)'\n');
						continue;
					case (byte)'r':
						ms.WriteByte((byte)'\r');
						continue;
					case (byte)'t':
						ms.WriteByte((byte)'\t');
						continue;
					case (byte)'u':
						if (i + 4 > content.Length)
							throw new InvalidOperationException("Unfinished escape sequence found?");

						unicodeChars = string.Empty;
					another_one:

						ushort utf16Char = 0;
						for (int n = 0; n < 4; n++)
						{
							utf16Char <<= 4; // move a nybble to the left
							byte hexDigit = content[i + n];
							switch (hexDigit)
							{
								case >= (byte)'0' and <= (byte)'9':
									utf16Char |= (byte)(hexDigit - (byte)'0');
									break;
								case >= (byte)'a' and <= (byte)'f':
									utf16Char |= (byte)(10 + (hexDigit - (byte)'a'));
									break;
								case >= (byte)'A' and <= (byte)'F':
									utf16Char |= (byte)(10 + (hexDigit - (byte)'A'));
									break;
								default:
									throw new InvalidOperationException("Assertion failure: Illegal character in hex escape sequence?");
							}
						}

						i += 4;
						unicodeChars += unchecked((char)utf16Char);

						// a single UTF-16 code point (i.e. encoded as \uFFFF) can span many 16 bit chars,
						// so if we find another UTF-16 encoded char following, accumulate it together
						// and then ask .NET to turn the (maybe multibyte) UTF-16 sequence into a
						// UTF-8 sequence
						if (
							i + @"\uFFFF"u8.Length <= content.Length &&
							content[i + 0] == (byte)'\\' &&
							content[i + 1] == (byte)'u'
						)
						{
							i += 2;
							goto another_one;
						}

						var utf8Bytes = Utf8.GetBytes(unicodeChars);
						ms.Write(utf8Bytes);
						continue;
					default:
						throw new InvalidOperationException("Invalid escape sequence?");
				}
			}
			else
			{
				ms.WriteByte(c);
				i++;
			}
		}

		return ms.ToArray();
	}

	public readonly bool TryReadUInt8(out byte value) => Utf8Parser.TryParse(TokenSourceUtf8, out value, out _);
	public readonly bool TryReadUInt16(out ushort value) => Utf8Parser.TryParse(TokenSourceUtf8, out value, out _);
	public readonly bool TryReadUInt32(out uint value) => Utf8Parser.TryParse(TokenSourceUtf8, out value, out _);
	public readonly bool TryReadUInt64(out ulong value) => Utf8Parser.TryParse(TokenSourceUtf8, out value, out _);
	public readonly bool TryReadInt8(out sbyte value) => Utf8Parser.TryParse(TokenSourceUtf8, out value, out _);
	public readonly bool TryReadInt16(out short value) => Utf8Parser.TryParse(TokenSourceUtf8, out value, out _);
	public readonly bool TryReadInt32(out int value) => Utf8Parser.TryParse(TokenSourceUtf8, out value, out _);
	public readonly bool TryReadInt64(out long value) => Utf8Parser.TryParse(TokenSourceUtf8, out value, out _);
	public readonly bool TryReadFloat32(out float value) => Utf8Parser.TryParse(TokenSourceUtf8, out value, out _);
	public readonly bool TryReadFloat64(out double value) => Utf8Parser.TryParse(TokenSourceUtf8, out value, out _);
}
