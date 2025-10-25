using System.Collections.Immutable;
using AspNetCore.Boilerplate.Roslyn.Constants;
using AspNetCore.Boilerplate.Roslyn.Diagnostics;
using AspNetCore.Boilerplate.Roslyn.Extensions;
using AspNetCore.Boilerplate.Roslyn.Helper;
using AspNetCore.Boilerplate.Roslyn.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AspNetCore.Boilerplate.Roslyn.Components.Service;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ServiceAnalyzer : DiagnosticAnalyzer
{
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeDependencySyntax, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeDependencySyntax(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ClassDeclarationSyntax)
            return;

        if (
            context.ContainingSymbol is not INamedTypeSymbol typeSymbol
            || !typeSymbol.HasAttributeWithFullyQualifiedMetadataName(
                $"{GeneratorConstant.LibNamespace}.ServiceAttribute"
            )
        )
            return;

        var typeName = typeSymbol.GetFullyQualifiedMetadataName();

        using var builder = ImmutableArrayBuilder<DiagnosticInfo>.Rent();

        if (typeSymbol is not { IsAbstract: false, IsGenericType: false })
            builder.Add(
                DiagnosticDescriptors.InvalidClassDecorationForDependency,
                typeSymbol,
                typeName
            );

        foreach (var diagnostic in builder.ToImmutable())
            context.ReportDiagnostic(diagnostic.ToDiagnostic());
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(DiagnosticDescriptors.InvalidClassDecorationForDependency);
}
