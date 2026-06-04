using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Yaseri.SourceGenerators;

internal static class StringExtension
{
	public static string NormalizeWhitespace(this string code)
	{
		return CSharpSyntaxTree.ParseText(code)
			.GetRoot()
			.NormalizeWhitespace(indentation: "\t")
			.ToFullString();
	}
}
