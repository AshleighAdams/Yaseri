using Microsoft.CodeAnalysis;
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
		//if (!Debugger.IsAttached)
		//	Debugger.Launch();

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
		var attributeObsolete = context.Compilation.GetTypeByMetadataName("System.ObsoleteAttribute")!;

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

			//AttributeData attributeData = typeDef
			//	.GetAttributes()
			//	.Single(ad => ad.AttributeClass!.Equals(attributeSymbol, SymbolEqualityComparer.Default));
			//var serializedName = attributeData.ConstructorArguments[0].Value!.ToString();

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
				if (property.GetAttributes().Any(ad => ad.AttributeClass!.Equals(attributeIgnore, SymbolEqualityComparer.Default))) continue;

				var propType = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
				var obsolete = property.GetAttributes().Any(ad => ad.AttributeClass!.Equals(attributeObsolete, SymbolEqualityComparer.Default));

				propertiesList.Add(new()
				{
					PropertyName = property.Name,
					Key = property.Name,
					FullType = propType,
					Obsolete = obsolete,
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
						"PX0001",
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
