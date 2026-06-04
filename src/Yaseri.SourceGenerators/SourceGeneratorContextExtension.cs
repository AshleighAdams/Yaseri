using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

using System.Text;

namespace Yaseri.SourceGenerators;

internal static class SourceGeneratorContextExtension
{
	public static void AddCode(this GeneratorExecutionContext context, string hintName, string code)
	{
		context.AddSource(
			hintName.Replace("<", "_").Replace(">", "_"),
			SourceText.From(code.NormalizeWhitespace(), Encoding.UTF8));
	}
}
