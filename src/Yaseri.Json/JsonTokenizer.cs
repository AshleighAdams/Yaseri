using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata;
using System.Text;

namespace Yaseri.Json;

[SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "Overzealous")]
public enum JsonTokenType
{
	Null,
	False,
	True,
	Number,
	String,
	StartArray,
	ValueSeparator,
	EndArray,
	StartObject,
	NameSeparator,
	EndObject,
}

public static class JsonTokenizerExtensions
{
	public static bool IsTokenValidStartValue(this JsonTokenType self)
	{
		return self switch
		{
			JsonTokenType.Null => true,
			JsonTokenType.False => true,
			JsonTokenType.True => true,
			JsonTokenType.Number => true,
			JsonTokenType.String => true,
			JsonTokenType.StartArray => true,
			JsonTokenType.StartObject => true,
			_ => false,
		};
	}
	public static int CountConsecutiveAnyOf(this ReadOnlySpan<byte> span, ReadOnlySpan<byte> set)
	{
		var pos = span.IndexOfAnyExcept(set);
		if (pos < 0)
			return span.Length;
		return pos;
	}
}

// https://www.ietf.org/rfc/rfc4627.txt
public class JsonTokenizer
{
	public static string PrettyFormatError(string message, ReadOnlySpan<byte> contents, string filename, int position)
	{
		var pos = Math.Min(position, contents.Length - 1);
		int columnOffset = 0;
		if (contents[pos] == '\n') // go back to previous line if we errored on a new line
		{
			var newPos = pos - 1;
			if (newPos < contents.Length)
			{
				if (newPos < pos)
					columnOffset = 1;
				pos = newPos;
			}
		}

		int lineStart, lineEnd;
		for (lineStart = pos; ; lineStart--)
		{
			if (lineStart == 0)
				break;
			else if (contents[lineStart] == '\n')
			{
				if (lineStart + 1 < contents.Length)
					lineStart++;
				break;
			}
		}
		for (lineEnd = pos; lineEnd < contents.Length; lineEnd++)
			if (contents[lineEnd] == '\r' || contents[lineEnd] == '\n')
				break;

		int lineNumber = 1;
		for (int i = 0; i < lineStart; i++)
			if (contents[i] == (byte)'\n')
				lineNumber++;
		int columnNumber = pos - lineStart + columnOffset + 1;
		var line = Encoding.UTF8.GetString(contents[lineStart..lineEnd]);
		string linePointer = new string(' ', columnNumber - 1);

		return $"{filename}:{lineNumber}:{columnNumber}: {message}\n{lineNumber,5:#0} | {line}\n      | {linePointer}^";
	}

	public ReadOnlyMemory<byte> Content  { get; set; }
	public int Position { get; set; }

	public JsonTokenizer(ReadOnlyMemory<byte> source)
	{
		Content = source;
		Position = 0;
	}

	public bool TryReadToken(out JsonToken token, out string? errorMessage)
	{
		errorMessage = null;
		token = default;

		Range range = default;
		while (true)
		{
			if (!CanRead)
				return false;

			switch (NextChar)
			{
				// handle whitespace
				case (byte)' ':
				case (byte)'\t':
				case (byte)'\r':
				case (byte)'\n':
					Consume(1);
					continue;

				// handle comments
				case (byte)'/':
					if (TryReadExact("//"u8, ref range))
					{
						throw new NotImplementedException();
						//continue;
					}
					else if (TryReadExact("/*"u8, ref range))
					{
						throw new NotImplementedException();
						//continue;
					}
					errorMessage = "Unexpected char found";
					return false;
				// handle strings
				case (byte)'"':
					if (TryReadString(ref range, out bool needsUnescaping, out errorMessage))
					{
						token = new(JsonTokenType.String, range, Content, needsUnescaping);
						return true;
					}
					return false;
				// handle numbers
				case (byte)'-':
				case (byte)'0':
				case (byte)'1':
				case (byte)'2':
				case (byte)'3':
				case (byte)'4':
				case (byte)'5':
				case (byte)'6':
				case (byte)'7':
				case (byte)'8':
				case (byte)'9':
					if (TryReadNumber(ref range, out errorMessage))
					{
						token = new(JsonTokenType.Number, range, Content);
						return true;
					}
					return false;
				// handle array operators
				case (byte)',':
					token = new(JsonTokenType.ValueSeparator, Consume(1), Content);
					return true;
				case (byte)'[':
					token = new(JsonTokenType.StartArray, Consume(1), Content);
					return true;
				case (byte)']':
					token = new(JsonTokenType.EndArray, Consume(1), Content);
					return true;

				// handle object operators
				case (byte)':':
					token = new(JsonTokenType.NameSeparator, Consume(1), Content);
					return true;
				case (byte)'{':
					token = new(JsonTokenType.StartObject, Consume(1), Content);
					return true;
				case (byte)'}':
					token = new(JsonTokenType.EndObject, Consume(1), Content);
					return true;

				case (byte)'n':
					if (TryReadExact("null"u8, ref range))
					{
						token = new(JsonTokenType.Null, range, Content);
						return true;
					}
					goto default;
				case (byte)'t':
					if (TryReadExact("true"u8, ref range))
					{
						token = new(JsonTokenType.True, range, Content);
						return true;
					}
					goto default;
				case (byte)'f':
					if (TryReadExact("false"u8, ref range))
					{
						token = new(JsonTokenType.False, range, Content);
						return true;
					}
					goto default;
				default:
					errorMessage = "Unexpected character";
					return false;
			}
		}
	}

	public static bool TryTokenize(ReadOnlyMemory<byte> json, IList<JsonToken> result)
	{
		var tokenizer = new JsonTokenizer(json);
		string? errMsg = null;
		while (tokenizer.TryReadToken(out var token, out errMsg))
			result.Add(token);

		return errMsg is null;
	}

	public bool CanRead => Position < Content.Length;
	public byte NextChar => Position < Content.Length ? Content.Span[Position] : (byte)0;
	public Range Consume(int length)
	{
		var start = Position;
		Position += length;
		return new(start, Position);
	}

	public bool TryReadExact(ReadOnlySpan<byte> value, ref Range srcRange)
	{
		if (Position + value.Length >= Content.Length)
			return false;

		if (!Content.Span[Position..].StartsWith(value))
			return false;
			
		srcRange = new(Position, Position + value.Length);
		Position += value.Length;
		return true;
	}

	public bool TryReadString(ref Range srcRange, out bool needsUnescaping, [NotNullWhen(false)] out string? errorMessage)
	{
		needsUnescaping = false;

		if (Position + 2 >= Content.Length)
		{
			errorMessage = "Unexpected EOF in string";
			return false;
		}
		var content = Content.Span;
		if (content[Position] != (byte)'"')
		{
			errorMessage = "Expected starting quote";
			return false;
		}

		bool endedString = false;

		int i = Position + 1;
		while (i < Content.Length)
		{
			byte c = content[i];
			if (c == (byte)'"')
			{
				endedString = true;
				i++;
				break;
			}
			else if (c == (byte)'\\')
			{
				needsUnescaping = true;
				i++;
				if (i >= Content.Length)
				{
					errorMessage = "Unexpected EOF after escape character";
					return false;
				}

				c = content[i];
				i++;

				switch (c)
				{
					case (byte)'"':
					case (byte)'\\':
					case (byte)'/':
					case (byte)'\'': // NOTE: "\'" is not technically legal json, but it is commonly found in the wild
						continue;
					case (byte)'b':
						continue;
					case (byte)'f':
						continue;
					case (byte)'n':
						continue;
					case (byte)'r':
						continue;
					case (byte)'t':
						continue;
					case (byte)'u':
						if (i + 4 >= Content.Length)
						{
							errorMessage = "Unexpected EOF in hex escape sequence";
							return false;
						}
						for (int n = 0; n < 4; n++)
						{
							i++;
							switch (content[i])
							{
								case > (byte)'0' and < (byte)'9':
								case > (byte)'a' and < (byte)'f':
								case > (byte)'A' and < (byte)'F':
									break;
								default:
									errorMessage = "Illegal character in hex escape sequence";
									return false;
							}
						}
						continue;
					default:
						errorMessage = "Invalid escape sequence";
						return false;
				}
			}
			else
			{
				i++;
			}
		}

		if (!endedString)
		{
			errorMessage = "Unterminated string";
			return false;
		}
		srcRange = new(Position, i);
		Position = i;
		errorMessage = null;
		return true;
	}

	public bool TryReadNumber(ref Range srcRange, [NotNullWhen(false)] out string? errorMessage)
	{
		var content = Content.Span;
		int i = Position;

		// handle optional minus
		if (content[i] == (byte)'-')
		{
			i++;
			if (i >= content.Length)
			{
				errorMessage = "Unexpected EOF";
				return false;
			}
		}

		// handle required int part
		var intSize = content[i..].CountConsecutiveAnyOf("0123456789"u8);
		if (intSize == 0)
		{
			errorMessage = "Expected one or more integer digits";
			return false;
		}
		var zerosAtStart = content[i..].CountConsecutiveAnyOf("0"u8);
		if ((intSize > 1 && zerosAtStart > 0) || (intSize == 1 && zerosAtStart > 1))
		{
			errorMessage = "Illegal leading zeros";
			return false;
		}
		i += intSize;

		// handle optional frac
		if (i < Content.Length && content[i] == (byte)'.')
		{
			i++;
			if (i >= Content.Length)
			{
				errorMessage = "Unexpected EOF";
				return false;
			}
			var fracCount = content[i..].CountConsecutiveAnyOf("0123456789"u8);
			if (fracCount == 0)
			{
				errorMessage = "Expected one or more fractional digits";
				return false;
			}
			i += fracCount;
		}

		// handle optional exponent
		if (i < Content.Length && content[i] is (byte)'e' or (byte)'E')
		{
			i++;
			if (i >= Content.Length)
			{
				errorMessage = "Unexpected EOF";
				return false;
			}
			if (content[i] is not (byte)'-' and not (byte)'+')
			{
				errorMessage = "Expected exponent +/-";
				return false;
			}
			i++;

			var expSize = content[i..].CountConsecutiveAnyOf("0123456789"u8);
			if (expSize == 0)
			{
				errorMessage = "Expected exponent digits";
				return false;
			}
			zerosAtStart = content[i..].CountConsecutiveAnyOf("0"u8);
			if ((expSize > 1 && zerosAtStart > 0) || (expSize == 1 && zerosAtStart > 1))
			{
				errorMessage = "Illegal leading zeros in exponent";
				return false;
			}
			i += expSize;
		}

		srcRange = new(Position, i);
		Position = i;
		errorMessage = null;
		return true;
	}
}
