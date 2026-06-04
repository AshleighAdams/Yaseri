using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yaseri.SourceGenerators;

[Generator]
public partial class YaseriSerializableGenerator : ISourceGenerator
{
	public void Initialize(GeneratorInitializationContext context)
	{
		//if (!System.Diagnostics.Debugger.IsAttached)
		//	System.Diagnostics.Debugger.Launch();

		//context.RegisterForPostInitialization((i) => i.AddSource("ComponentSerializableAttribute.g.cs", AttributeSource));

		// Register a syntax receiver that will be created for each generation pass
		context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
	}

	public void Execute(GeneratorExecutionContext context)
	{
		if (context.SyntaxContextReceiver is not SyntaxReceiver receiver)
			return;

		var attributeSymbol = context.Compilation.GetTypeByMetadataName("Yaseri.Attributes.YaseriSerializableAttribute")!;
		var attributeIgnore = context.Compilation.GetTypeByMetadataName("Yaseri.Attributes.IgnoreAttribute")!;
		var attributePropName = context.Compilation.GetTypeByMetadataName("Yaseri.Attributes.PropertyNameAttribute")!;
		var attributeUsage = context.Compilation.GetTypeByMetadataName("Yaseri.Attributes.UsageAttribute")!;
		var attributeFormat = context.Compilation.GetTypeByMetadataName("Yaseri.Attributes.FormatAttribute")!;
		var attributeConstraints = context.Compilation.GetTypeByMetadataName("Yaseri.Attributes.ConstraintsAttribute")!;
		var attributeObsolete = context.Compilation.GetTypeByMetadataName("System.ObsoleteAttribute")!;

		string writeInline = "false";

		foreach (var (typeDef, classDef) in receiver.CandidateTypes)
		{
			if (typeDef.IsAbstract)
			{
				context.ReportDiagnostic(Diagnostic.Create(
					new DiagnosticDescriptor(
						"YASI0001",
						"YaseriSerializable applied to an abstract class.",
						"Type {0} is abstract. YaseriSerializable can onyl be applied to concrete types.",
						"error",
						DiagnosticSeverity.Error,
						true),
					typeDef.Locations.FirstOrDefault(),
					typeDef.Name));
			}

			AttributeData attributeData = typeDef
				.GetAttributes()
				.Single(ad => ad.AttributeClass!.Equals(attributeSymbol, SymbolEqualityComparer.Default));
			foreach (var kv in attributeData.NamedArguments)
			{
				switch (kv.Key)
				{
					case "WriteInline":
						writeInline = kv.Value.ToCSharpString();
						break;
					default:
						break;
				}
			}

			var sb = new StringBuilder();

			var propertiesList = new List<PropertyMetadata>();

			var properties = typeDef.GetMembers().OfType<IPropertySymbol>();
			foreach (var property in properties)
			{
				if (property.CanBeReferencedByName != true) continue;
				if (property.IsImplicitlyDeclared != false) continue;
				if (property.DeclaredAccessibility != Accessibility.Public) continue;
				if (property.IsStatic != false) continue;
				if (property.SetMethod is null) continue;

				bool ignore = false;
				bool obsolete = false;
				string? key = null;
				string? format = null;
				string? min = null;
				string? max = null;
				string? step = null;
				List<string>? usage = null;

				foreach (var attribute in property.GetAttributes())
				{
					if (attribute.AttributeClass is not INamedTypeSymbol attributeClass)
						continue;

					if (attributeClass.Equals(attributeIgnore, SymbolEqualityComparer.Default))
					{
						ignore = true;
						break;
					}
					else if (attributeClass.Equals(attributeObsolete, SymbolEqualityComparer.Default))
					{
						obsolete = true;
					}
					else if (attributeClass.Equals(attributePropName, SymbolEqualityComparer.Default))
					{
						key = attribute.ConstructorArguments[0].ToCSharpString();
					}
					else if (attributeClass.Equals(attributeFormat, SymbolEqualityComparer.Default))
					{
						format = attribute.ConstructorArguments[0].ToCSharpString();
					}
					else if (attributeClass.Equals(attributeUsage, SymbolEqualityComparer.Default))
					{
						var hint = attribute.ConstructorArguments[0].ToCSharpString();
						usage ??= [];
						usage.Add(hint);
					}
					else if (attributeClass.Equals(attributeConstraints, SymbolEqualityComparer.Default))
					{
						foreach (var kv in attribute.NamedArguments)
						{
							switch (kv.Key)
							{
								case "Minimum":
									min = kv.Value.ToCSharpString();
									break;
								case "Maximum":
									max = kv.Value.ToCSharpString();
									break;
								case "Step":
									step = kv.Value.ToCSharpString();
									break;
								default:
									break;
							}
						}
					}
				}

				if (ignore)
					continue;

				var propType = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

				propertiesList.Add(new()
				{
					PropertyName = property.Name,
					Key = key ?? $"\"{property.Name.ToLiteral()}\"",
					FullType = propType,
					Obsolete = obsolete,
					FormatHint = format,
					CustomHints = usage,
					ConstraintMin = min,
					ConstraintMax = max,
					ConstraintStep = step,
				});
			}

			string declaringNamespace = typeDef.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
			string fullTypeName = typeDef.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

			//bool isSerializable = classDef.BaseList.Types
			//	.Select(t => t.ToString());
			//	.
			//	typeDef.BaseType
			//		.GetAttributes()
			//		.Any(ad => ad.AttributeClass!.Equals(attributeSymbol, SymbolEqualityComparer.Default));

			string? baseClass = typeDef.BaseType?.Name ?? null;

			var visibility = typeDef.DeclaredAccessibility switch
			{
				Accessibility.Private => null,
				Accessibility.ProtectedAndInternal => "internal",
				Accessibility.Protected => null,
				Accessibility.Internal => "internal",
				Accessibility.ProtectedOrInternal => "internal",
				Accessibility.Public => "public",
				_ => null,
			};

			if (visibility is null)
			{
				context.ReportDiagnostic(Diagnostic.Create(
					new DiagnosticDescriptor(
						"YASI0002",
						"Accessibility must be public or internal",
						"Component {0} is {1}. All serializable components must be public or internal.",
						"error",
						DiagnosticSeverity.Error,
						true),
					typeDef.Locations.FirstOrDefault(),
					typeDef.Name,
					typeDef.DeclaredAccessibility.ToString()));
				continue;
			}
			sb.Generate(
				visibility,
				declaringNamespace,
				baseClass,
				fullTypeName,
				typeDef.Name,
				writeInline,
				propertiesList);

			context.AddCode($"{typeDef.Name}.g.cs", sb.ToString());
		}
	}

	/// <summary>
	/// Created on demand before each generation pass
	/// </summary>
	internal class SyntaxReceiver : ISyntaxContextReceiver
	{
		public List<(INamedTypeSymbol typeSymbol, ClassDeclarationSyntax classDecl)> CandidateTypes { get; } = new();

		public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
		{
			if (context.Node is ClassDeclarationSyntax classDeclarationSyntax
				&& classDeclarationSyntax.AttributeLists.Count > 0)
			{
				var symbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax) as INamedTypeSymbol;
				bool valid = symbol!.GetAttributes()
					.Any(ad => ad.AttributeClass?.ToDisplayString() == "Yaseri.Attributes.YaseriSerializableAttribute");

				if (valid)
					CandidateTypes.Add((symbol, classDeclarationSyntax));
			}

			//if (context.Node is FieldDeclarationSyntax fieldDeclarationSyntax
			//	&& fieldDeclarationSyntax.AttributeLists.Count > 0)
			//{
			//	foreach (VariableDeclaratorSyntax variable in fieldDeclarationSyntax.Declaration.Variables)
			//	{
			//		// Get the symbol being declared by the field, and keep it if its annotated
			//		IFieldSymbol fieldSymbol = context.SemanticModel.GetDeclaredSymbol(variable) as IFieldSymbol;
			//		if (fieldSymbol.GetAttributes().Any(ad => ad.AttributeClass.ToDisplayString() == "AutoNotify.AutoNotifyAttribute"))
			//		{
			//			Fields.Add(fieldSymbol);
			//		}
			//	}
			//}
		}
	}
}
