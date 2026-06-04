using System;
using System.Collections.Generic;
using System.IO;

namespace Yaseri.Json;

public record struct JsonPrimitiveReaderOptions
{
	public JsonPrimitiveReaderOptions() { }
}

public sealed class JsonPrimitiveReader : IPrimitiveReader
{
	private JsonTokenizer Tokenizer { get; }
	private string Filename { get; set; }

	public JsonPrimitiveReader(ReadOnlyMemory<byte> jsonSource, string filename, JsonPrimitiveReaderOptions? options = default)
	{
		options ??= new();
		Tokenizer = new(jsonSource);
		Filename = filename;
	}

	private enum ParseState
	{
		Value,
		ObjectKey,
		ObjectValue,
		ArrayValue,
		Complete,
	}

	private Stack<ParseState> StateStack { get; } = new();
	private ParseState State { get; set; } = ParseState.Value;

	public long CurrentPosition => Tokenizer.Position;
	public string? LastError { get; set; }

	private readonly List<JsonToken> TokenQueue = new();

	private bool PeekTokens(int count)
	{
		while (TokenQueue.Count < count)
		{
			if (!Tokenizer.TryReadToken(out var token, out var errMsg))
			{
				if (errMsg is null)
					return false;
				string msg = JsonTokenizer.PrettyFormatError(errMsg, Tokenizer.Content.Span, Filename, Tokenizer.Position);
				throw new InvalidDataException(msg);
			}
			TokenQueue.Add(token);
		}
		return true;
	}

	private void FlushQueue(int count)
	{
		TokenQueue.RemoveRange(0, count);
	}

	private bool TryStartValue()
	{
		if (State is not ParseState.ArrayValue and not ParseState.ObjectValue and not ParseState.Value)
		{
			LastError = "Unexpected value position";
			return false;
		}
		if (!PeekTokens(1))
		{
			LastError = "Unexpected EOF";
			return false;
		}
		return true;
	}

	private void CompleteValue()
	{
		if (State is ParseState.ObjectValue or ParseState.ArrayValue)
		{
			if (!PeekTokens(1))
			{
				string msg = JsonTokenizer.PrettyFormatError("Unexpected EOF", Tokenizer.Content.Span, Filename, Tokenizer.Content.Length);
				throw new InvalidDataException(msg);
			}

			switch ((State, TokenQueue[0].Type))
			{
				case (ParseState.ObjectValue, JsonTokenType.ValueSeparator):
					State = ParseState.ObjectKey;
					FlushQueue(1);
					break;
				case (ParseState.ArrayValue, JsonTokenType.ValueSeparator):
					FlushQueue(1);
					break;
				case (ParseState.ObjectValue, JsonTokenType.EndObject):
					State = ParseState.ObjectKey;
					break;
				case (ParseState.ArrayValue, JsonTokenType.EndArray):
					break;
				default:
					string msg = JsonTokenizer.PrettyFormatError("Expected value separator or terminator", Tokenizer.Content.Span, Filename, TokenQueue[0].Location.Start.Value);
					throw new InvalidDataException(msg);
			}
		}
		else if (State == ParseState.ObjectKey)
		{
			string msg = JsonTokenizer.PrettyFormatError("Attempting to complete name as value", Tokenizer.Content.Span, Filename, TokenQueue[0].Location.Start.Value);
			throw new InvalidOperationException(msg);
		}
		else
			State = ParseState.Complete;
	}

	public bool TryReadStartObject()
	{
		if (!TryStartValue())
			return false;

		if (TokenQueue[0].Type != JsonTokenType.StartObject)
		{
			LastError = "Expected {";
			return false;
		}

		StateStack.Push(State);
		State = ParseState.ObjectKey;

		FlushQueue(1);
		return true;
	}

	public bool TryReadKey(ReadOnlySpan<byte> expectedKey)
	{
		if (State != ParseState.ObjectKey)
		{
			LastError = "Unexpected key position";
			return false;
		}

		if (!PeekTokens(3))
		{
			LastError = "Expected name, name separator, then value, got EOF";
			return false;
		}

		bool tokensCorrectType =
			TokenQueue[0].Type is JsonTokenType.String &&
			TokenQueue[1].Type is JsonTokenType.NameSeparator &&
			TokenQueue[2].Type.IsTokenValidStartValue();
		if (!tokensCorrectType)
		{
			LastError = "Expected name, name separator, then value";
			return false;
		}

		if (TokenQueue[0] != expectedKey)
		{
			LastError = "Expected name, name separator, then value";
			return false;
		}

		FlushQueue(2);
		State = ParseState.ObjectValue;
		return true;
	}

	public bool TryReadKey(out ReadOnlyMemory<byte> key)
	{
		if (State != ParseState.ObjectKey)
		{
			LastError = "Unexpected key position";
			key = Array.Empty<byte>();
			return false;
		}

		if (!PeekTokens(3))
		{
			LastError = "Expected name, name separator, then value, got EOF";
			key = Array.Empty<byte>();
			return false;
		}

		bool tokensCorrectType =
			TokenQueue[0].Type is JsonTokenType.String &&
			TokenQueue[1].Type is JsonTokenType.NameSeparator &&
			TokenQueue[2].Type.IsTokenValidStartValue();
		if (!tokensCorrectType)
		{
			LastError = "Expected name, name separator, then value";
			key = Array.Empty<byte>();
			return false;
		}

		key = TokenQueue[0].ReadUtf8String();
		FlushQueue(2);
		State = ParseState.ObjectValue;
		return true;
	}

	public bool SkipThisValue()
	{
		int depth = 1;
		while (true)
		{
			if (!PeekTokens(1))
			{
				LastError = "Unexpected EOF";
				return false;
			}

			switch (TokenQueue[0].Type)
			{
				case JsonTokenType.StartObject:
				case JsonTokenType.StartArray:
					depth++;
					goto default;

				case JsonTokenType.EndObject:
				case JsonTokenType.EndArray:
					depth--;
					if (depth == 0)
						return true;
					goto default;
				default:
					FlushQueue(1);
					break;
			}

		}
	}

	public bool TryReadEndObject(bool skipToEnd = false)
	{
		if (State is not ParseState.ObjectKey)
		{
			LastError = "Unexpected end object position";
			return false;
		}

		if (skipToEnd && !SkipThisValue())
		{
			LastError = "Failed to skip object, expected }";
			return false;
		}

		if (!PeekTokens(1) || TokenQueue[0].Type != JsonTokenType.EndObject)
		{
			LastError = "Expected }";
			return false;
		}

		State = StateStack.Pop();
		FlushQueue(1);
		CompleteValue();
		return true;
	}

	public bool TryReadStartArray()
	{
		if (!TryStartValue())
			return false;

		if (TokenQueue[0].Type != JsonTokenType.StartArray)
		{
			LastError = "Expected [";
			return false;
		}

		StateStack.Push(State);
		State = ParseState.ArrayValue;

		FlushQueue(1);
		return true;
	}

	public bool TryReadEndArray(bool skipToEnd = false)
	{
		if (State is not ParseState.ArrayValue)
		{
			LastError = "Unexpected end array position";
			return false;
		}

		if (skipToEnd && !SkipThisValue())
		{
			LastError = "Failed to skip object, expected }";
			return false;
		}

		if (!PeekTokens(1) || TokenQueue[0].Type != JsonTokenType.EndArray)
		{
			LastError = "Expected ]";
			return false;
		}

		State = StateStack.Pop();
		FlushQueue(1);
		CompleteValue();
		return true;
	}

	public bool TrySkipValue()
	{
		if (!TryStartValue())
			return false;

		var valueType = TokenQueue[0].Type;
		switch (valueType)
		{
			case JsonTokenType.False:
			case JsonTokenType.True:
			case JsonTokenType.String:
			case JsonTokenType.Number:
			case JsonTokenType.Null:
				break;
			case JsonTokenType.StartArray:
			case JsonTokenType.StartObject:
				FlushQueue(1);
				if (!SkipThisValue())
					return false;

				PeekTokens(1);
				var endType = TokenQueue[0].Type;
				if (valueType == JsonTokenType.StartObject && endType != JsonTokenType.EndObject)
				{
					LastError = "Expected }";
					return false;
				}
				else if (valueType == JsonTokenType.StartArray && endType != JsonTokenType.EndArray)
				{
					LastError = "Expected ]";
					return false;
				}
				break;
			default:
				LastError = "Unexpected token type";
				return false;
		}

		FlushQueue(1);
		CompleteValue();
		return true;
	}

	public bool TryReadNull()
	{
		if (!TryStartValue())
			return false;

		if (TokenQueue[0].Type != JsonTokenType.Null)
			return false;

		FlushQueue(1);
		CompleteValue();
		return true;
	}

	public bool TryReadValue(out bool value)
	{
		if (!TryStartValue())
		{
			value = default;
			return false;
		}

		switch (TokenQueue[0].Type)
		{
			case JsonTokenType.False:
				value = false;
				break;
			case JsonTokenType.True:
				value = true;
				break;
			default:
				LastError = "Expected true or false";
				value = default;
				return false;
		}

		FlushQueue(1);
		CompleteValue();
		return true;
	}

	public bool TryReadValue(out sbyte value)
	{
		if (!TryStartValue())
		{
			value = default;
			return false;
		}

		if (TokenQueue[0].Type != JsonTokenType.Number)
		{
			LastError = "Expected number";
			value = default;
			return false;
		}

		if (!TokenQueue[0].TryReadInt8(out value))
		{
			LastError = "Failed to parse int8";
			value = default;
			return false;
		}

		FlushQueue(1);
		CompleteValue();
		return true;
	}

	public bool TryReadValue(out short value)
	{
		if (!TryStartValue())
		{
			value = default;
			return false;
		}

		if (TokenQueue[0].Type != JsonTokenType.Number)
		{
			LastError = "Expected number";
			value = default;
			return false;
		}

		if (!TokenQueue[0].TryReadInt16(out value))
		{
			LastError = "Failed to parse int16";
			value = default;
			return false;
		}

		FlushQueue(1);
		CompleteValue();
		return true;
	}

	public bool TryReadValue(out int value)
	{
		if (!TryStartValue())
		{
			value = default;
			return false;
		}

		if (TokenQueue[0].Type != JsonTokenType.Number)
		{
			LastError = "Expected number";
			value = default;
			return false;
		}

		if (!TokenQueue[0].TryReadInt32(out value))
		{
			LastError = "Failed to parse int32";
			value = default;
			return false;
		}

		FlushQueue(1);
		CompleteValue();
		return true;
	}

	public bool TryReadValue(out long value)
	{
		if (!TryStartValue())
		{
			value = default;
			return false;
		}

		if (TokenQueue[0].Type != JsonTokenType.Number)
		{
			LastError = "Expected number";
			value = default;
			return false;
		}

		if (!TokenQueue[0].TryReadInt64(out value))
		{
			LastError = "Failed to parse int64";
			value = default;
			return false;
		}

		FlushQueue(1);
		CompleteValue();
		return true;
	}

	public bool TryReadValue(out byte value)
	{
		if (!TryStartValue())
		{
			value = default;
			return false;
		}

		if (TokenQueue[0].Type != JsonTokenType.Number)
		{
			LastError = "Expected number";
			value = default;
			return false;
		}

		if (!TokenQueue[0].TryReadUInt8(out value))
		{
			LastError = "Failed to parse byte";
			value = default;
			return false;
		}

		FlushQueue(1);
		CompleteValue();
		return true;
	}

	public bool TryReadValue(out ushort value)
	{
		if (!TryStartValue())
		{
			value = default;
			return false;
		}

		if (TokenQueue[0].Type != JsonTokenType.Number)
		{
			LastError = "Expected number";
			value = default;
			return false;
		}

		if (!TokenQueue[0].TryReadUInt16(out value))
		{
			LastError = "Failed to parse uint16";
			value = default;
			return false;
		}

		FlushQueue(1);
		CompleteValue();
		return true;
	}

	public bool TryReadValue(out uint value)
	{
		if (!TryStartValue())
		{
			value = default;
			return false;
		}

		if (TokenQueue[0].Type != JsonTokenType.Number)
		{
			LastError = "Expected number";
			value = default;
			return false;
		}

		if (!TokenQueue[0].TryReadUInt32(out value))
		{
			LastError = "Failed to parse uint32";
			value = default;
			return false;
		}

		FlushQueue(1);
		CompleteValue();
		return true;
	}

	public bool TryReadValue(out ulong value)
	{
		if (!TryStartValue())
		{
			value = default;
			return false;
		}

		if (TokenQueue[0].Type != JsonTokenType.Number)
		{
			LastError = "Expected number";
			value = default;
			return false;
		}

		if (!TokenQueue[0].TryReadUInt64(out value))
		{
			LastError = "Failed to parse uint64";
			value = default;
			return false;
		}

		FlushQueue(1);
		CompleteValue();
		return true;
	}

	public bool TryReadValue(out float value)
	{
		if (!TryStartValue())
		{
			value = default;
			return false;
		}

		if (TokenQueue[0].Type != JsonTokenType.Number)
		{
			LastError = "Expected number";
			value = default;
			return false;
		}

		if (!TokenQueue[0].TryReadFloat32(out value))
		{
			LastError = "Failed to parse float32";
			value = default;
			return false;
		}

		FlushQueue(1);
		CompleteValue();
		return true;
	}

	public bool TryReadValue(out double value)
	{
		if (!TryStartValue())
		{
			value = default;
			return false;
		}

		if (TokenQueue[0].Type != JsonTokenType.Number)
		{
			LastError = "Expected number";
			value = default;
			return false;
		}

		if (!TokenQueue[0].TryReadFloat64(out value))
		{
			LastError = "Failed to parse float64";
			value = default;
			return false;
		}

		FlushQueue(1);
		CompleteValue();
		return true;
	}

	public bool TryReadValue(out string value)
	{
		if (!TryStartValue())
		{
			value = string.Empty;
			return false;
		}

		if (TokenQueue[0].Type != JsonTokenType.String)
		{
			LastError = "Expected string";
			value = string.Empty;
			return false;
		}

		value = TokenQueue[0].ReadString();
		FlushQueue(1);
		CompleteValue();
		return true;
	}

	public bool TryReadValue(out byte[] value)
	{
		if (!TryStartValue())
		{
			value = Array.Empty<byte>();
			return false;
		}

		if (TokenQueue[0].Type != JsonTokenType.String)
		{
			LastError = "Expected base64 encoded string";
			value = Array.Empty<byte>();
			return false;
		}

		try
		{
			value = Convert.FromBase64String(TokenQueue[0].ReadString());
		}
		catch (FormatException)
		{
			LastError = "Failed to parse base64 encoded string";
			value = Array.Empty<byte>();
			return false;
		}

		FlushQueue(1);
		CompleteValue();
		return true;
	}
}
