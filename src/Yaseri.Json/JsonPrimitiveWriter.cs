using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace Yaseri.Json;

public record struct JsonPrimitiveWriterOptions
{
	public JsonPrimitiveWriterOptions() { }
	public bool WriteTrailingCommas { get; set; } = true;
	public bool Indent { get; set; } = true;
	public string IndentationLiteral { get; set; } = "\t";
	public string NewlineLiteral { get; set; } = "\n";
	public string InlineNewlineLiteral { get; set; } = " ";
}

public sealed class JsonPrimitiveWriter : IPrimitiveWriter, IDisposable
{
	private static readonly UTF8Encoding Utf8 = new(encoderShouldEmitUTF8Identifier: false);
	private MemoryStream Stream { get; } = new();
	private readonly byte[] ValueFormatBuffer = new byte[512];

	public JsonPrimitiveWriter(JsonPrimitiveWriterOptions? options = default)
	{
		var opts = options ?? new();
		WriteTrailingCommas = opts.WriteTrailingCommas;
		Indent = opts.Indent;
		var indents = DoubleDoubleDoubleBytes(opts.IndentationLiteral);
		OneIndentationLiteral = indents.one;
		TwoIndentationLiterals = indents.two;
		FourIndentationLiterals = indents.four;
		EightIndentationLiterals = indents.eight;
		NewlineLiteral = Utf8.GetBytes(opts.NewlineLiteral);
		InlineNewlineLiteral = Utf8.GetBytes(opts.InlineNewlineLiteral);
	}

	private static (Memory<byte> one, Memory<byte> two, Memory<byte> four, Memory<byte> eight) DoubleDoubleDoubleBytes(string value)
	{
		var bytesForOne = Utf8.GetByteCount(value);
		var eight = new byte[bytesForOne * 8];
#if DEBUG
		var count =
#endif
		Utf8.GetBytes(value, eight);
#if DEBUG
		Debug.Assert(count == bytesForOne);
#endif
		for (int i = 0; i < bytesForOne; i++)
		{
			eight[bytesForOne * 1 + i] = eight[i];
			eight[bytesForOne * 2 + i] = eight[i];
			eight[bytesForOne * 3 + i] = eight[i];
			eight[bytesForOne * 4 + i] = eight[i];
			eight[bytesForOne * 5 + i] = eight[i];
			eight[bytesForOne * 6 + i] = eight[i];
			eight[bytesForOne * 7 + i] = eight[i];
		}

		var mem = eight.AsMemory();
		return (mem[..bytesForOne], mem[..(bytesForOne*2)], mem[..(bytesForOne * 4)], mem[..(bytesForOne * 8)]);
	}

	public bool WriteTrailingCommas { get; set; } = true;
	public bool Indent { get; set; }

	private Memory<byte> OneIndentationLiteral { get; }
	private Memory<byte> TwoIndentationLiterals { get; }
	private Memory<byte> FourIndentationLiterals { get; }
	private Memory<byte> EightIndentationLiterals { get; }
	private Memory<byte> NewlineLiteral { get; }
	private Memory<byte> InlineNewlineLiteral { get; }

	public long CurrentPosition => Stream.Position;
	public bool ShouldWriteDefaultValues { get; set; }

	private enum WriteState
	{
		Value,
		ObjectKeyFirst,
		ObjectKey,
		ObjectValue,
		ArrayValueFirst,
		ArrayValue,
		Complete,
	}

	private int WriteInlineDepth { get; set; }
	private int Depth => StateStack.Count;

	private bool WritingInline => !Indent || WriteInlineDepth > 0;

	private Stack<WriteState> StateStack { get; } = new();
	private WriteState State { get; set; } = WriteState.Value;

	private void WriteNewline(bool starting = false, bool terminating = false)
	{
		if (WritingInline)
		{
			if (starting || terminating)
				return;
			Stream.Write(InlineNewlineLiteral.Span);
		}
		else
		{
			Stream.Write(NewlineLiteral.Span);
			int remainingDepth = Depth;
			if (terminating)
				remainingDepth--;
			while (remainingDepth > 0)
			{
				switch (remainingDepth)
				{
					default:
					case >= 8:
						Stream.Write(EightIndentationLiterals.Span);
						remainingDepth -= 8;
						break;
					case >= 4:
						Stream.Write(FourIndentationLiterals.Span);
						remainingDepth -= 4;
						break;
					case >= 2:
						Stream.Write(TwoIndentationLiterals.Span);
						remainingDepth -= 2;
						break;
					case 1:
						Stream.Write(OneIndentationLiteral.Span);
						remainingDepth = 0;
						break;
				}
			}
		}
	}

	private void StartValue()
	{
		switch (State)
		{
			case WriteState.Value:
			case WriteState.ObjectValue:
				break;

			case WriteState.ArrayValueFirst:
				WriteNewline(starting: true);
				break;

			case WriteState.ArrayValue:
				Stream.WriteByte((byte)',');
				WriteNewline(starting: false);
				break;

			case WriteState.ObjectKeyFirst:
			case WriteState.ObjectKey:
			case WriteState.Complete:
				throw new InvalidOperationException("Attempt to start value in incorrect state");
			default:
				throw new NotImplementedException();
		}
	}

	private void CompleteValue()
	{
		switch (State)
		{
			case WriteState.Value:
				State = WriteState.Complete;
				break;
			case WriteState.ObjectValue:
				State = WriteState.ObjectKey;
				break;
			case WriteState.ArrayValueFirst:
				State = WriteState.ArrayValue;
				break;
			case WriteState.ArrayValue:
				break;
			case WriteState.ObjectKeyFirst:
			case WriteState.ObjectKey:
			case WriteState.Complete:
				throw new InvalidOperationException("Attempt to complete value in incorrect state");
			default:
				throw new NotImplementedException();
		}
	}

	public void WriteStartObject(bool writeInline = false)
	{
		StartValue();
		Stream.WriteByte((byte)'{');

		StateStack.Push(State);
		State = WriteState.ObjectKeyFirst;

		if (writeInline && WriteInlineDepth == 0)
			WriteInlineDepth = Depth;
	}

	public void WriteKey(ReadOnlySpan<byte> key)
	{
		switch (State)
		{
			case WriteState.ObjectKeyFirst:
				WriteNewline(starting: true);
				break;
			case WriteState.ObjectKey:
				Stream.WriteByte((byte)',');
				WriteNewline();
				break;
			case WriteState.ObjectValue:
			case WriteState.ArrayValueFirst:
			case WriteState.ArrayValue:
			case WriteState.Value:
			case WriteState.Complete:
				throw new InvalidOperationException("Attempt to write key in incorrect state");
			default:
				throw new NotImplementedException();
		}

		// TODO: escape
		Stream.WriteByte((byte)'"');
		Stream.Write(key);
		Stream.Write("\": "u8);
		State = WriteState.ObjectValue;
	}

	public void WriteEndObject()
	{
		if (State is not WriteState.ObjectKeyFirst and not WriteState.ObjectKey)
			throw new InvalidOperationException("Invalid state to end object");

		if (State != WriteState.ObjectKeyFirst)
		{
			if (WriteTrailingCommas && !WritingInline)
				Stream.WriteByte((byte)',');
			WriteNewline(terminating: true);
		}
		Stream.WriteByte((byte)'}');

		if (Depth == WriteInlineDepth)
			WriteInlineDepth = 0;

		State = StateStack.Pop();
		CompleteValue();
	}

	public void WriteStartArray(bool writeInline = false)
	{
		StartValue();
		Stream.WriteByte((byte)'[');

		StateStack.Push(State);
		State = WriteState.ArrayValueFirst;

		if (writeInline && WriteInlineDepth == 0)
			WriteInlineDepth = Depth;
	}

	public void WriteEndArray()
	{
		if (State is not WriteState.ArrayValueFirst and not WriteState.ArrayValue)
			throw new InvalidOperationException("Invalid state to end array");

		if (State != WriteState.ArrayValueFirst)
		{
			if (WriteTrailingCommas && !WritingInline)
				Stream.WriteByte((byte)',');
			WriteNewline(terminating: true);
		}
		Stream.WriteByte((byte)']');

		if (Depth == WriteInlineDepth)
			WriteInlineDepth = 0;

		State = StateStack.Pop();
		CompleteValue();
	}

	public void WriteNull()
	{
		StartValue();
		Stream.Write("null"u8);
		CompleteValue();
	}

	public void WriteValue(bool value)
	{
		StartValue();
		Stream.Write(value ? "true"u8 : "false"u8);
		CompleteValue();
	}

	public void WriteValue(sbyte value)
	{
		Utf8Formatter.TryFormat(value, ValueFormatBuffer, out int bytesWritten);
		var valueUtf8Str = ValueFormatBuffer[..bytesWritten];

		StartValue();
		Stream.Write(valueUtf8Str);
		CompleteValue();
	}

	public void WriteValue(short value)
	{
		Utf8Formatter.TryFormat(value, ValueFormatBuffer, out int bytesWritten);
		var valueUtf8Str = ValueFormatBuffer[..bytesWritten];

		StartValue();
		Stream.Write(valueUtf8Str);
		CompleteValue();
	}

	public void WriteValue(int value)
	{
		Utf8Formatter.TryFormat(value, ValueFormatBuffer, out int bytesWritten);
		var valueUtf8Str = ValueFormatBuffer[..bytesWritten];

		StartValue();
		Stream.Write(valueUtf8Str);
		CompleteValue();
	}

	public void WriteValue(long value)
	{
		Utf8Formatter.TryFormat(value, ValueFormatBuffer, out int bytesWritten);
		var valueUtf8Str = ValueFormatBuffer[..bytesWritten];

		StartValue();
		Stream.Write(valueUtf8Str);
		CompleteValue();
	}

	public void WriteValue(byte value)
	{
		Utf8Formatter.TryFormat(value, ValueFormatBuffer, out int bytesWritten);
		var valueUtf8Str = ValueFormatBuffer[..bytesWritten];

		StartValue();
		Stream.Write(valueUtf8Str);
		CompleteValue();
	}

	public void WriteValue(ushort value)
	{
		Utf8Formatter.TryFormat(value, ValueFormatBuffer, out int bytesWritten);
		var valueUtf8Str = ValueFormatBuffer[..bytesWritten];

		StartValue();
		Stream.Write(valueUtf8Str);
		CompleteValue();
	}

	public void WriteValue(uint value)
	{
		Utf8Formatter.TryFormat(value, ValueFormatBuffer, out int bytesWritten);
		var valueUtf8Str = ValueFormatBuffer[..bytesWritten];

		StartValue();
		Stream.Write(valueUtf8Str);
		CompleteValue();
	}

	public void WriteValue(ulong value)
	{
		Utf8Formatter.TryFormat(value, ValueFormatBuffer, out int bytesWritten);
		var valueUtf8Str = ValueFormatBuffer[..bytesWritten];

		StartValue();
		Stream.Write(valueUtf8Str);
		CompleteValue();
	}

	public void WriteValue(float value)
	{
		Utf8Formatter.TryFormat(value, ValueFormatBuffer, out int bytesWritten);
		var valueUtf8Str = ValueFormatBuffer[..bytesWritten];

		StartValue();
		Stream.Write(valueUtf8Str);
		CompleteValue();
	}

	public void WriteValue(double value)
	{
		Utf8Formatter.TryFormat(value, ValueFormatBuffer, out int bytesWritten);
		var valueUtf8Str = ValueFormatBuffer[..bytesWritten];

		StartValue();
		Stream.Write(valueUtf8Str);
		CompleteValue();
	}

	public void WriteValue(char value)
	{
		Utf8Formatter.TryFormat(value, ValueFormatBuffer, out int bytesWritten);
		var valueUtf8Str = ValueFormatBuffer[..bytesWritten];
		WriteValue(value);
	}

	public void WriteValue(string value)
	{
		var valueUtf8Str = Utf8.GetBytes(value);

		StartValue();
		Stream.WriteByte((byte)'"');
		Stream.Write(valueUtf8Str); // TODO: escape strings
		Stream.WriteByte((byte)'"');
		CompleteValue();
	}

	public void WriteValue(byte[] value)
	{
		WriteValue(Convert.ToBase64String(value));
	}

	public Memory<byte> GetJson()
	{
		Stream.Flush();
		return Stream.ToArray();
	}

	public override string ToString()
	{
		return Utf8.GetString(GetJson().Span);
	}

	public void Dispose()
	{
		Stream.Dispose();
	}
}
