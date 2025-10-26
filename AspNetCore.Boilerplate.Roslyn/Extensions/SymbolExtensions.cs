using System.Collections.Immutable;
using AspNetCore.Boilerplate.Roslyn.Helper;
using Microsoft.CodeAnalysis;

namespace AspNetCore.Boilerplate.Roslyn.Extensions;

internal static class SymbolExtensions
{
    public static string GetFullyQualifiedMetadataName(this ITypeSymbol symbol)
    {
        using var builder = ImmutableArrayBuilder<char>.Rent();

        symbol.AppendFullyQualifiedMetadataName(in builder);

        return builder.ToString();
    }

    public static string GetFullyQualifiedName(this ISymbol symbol)
    {
        return symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    public static bool HasFullyQualifiedMetadataName(this ITypeSymbol symbol, string name)
    {
        using var builder = ImmutableArrayBuilder<char>.Rent();

        symbol.AppendFullyQualifiedMetadataName(in builder);

        return builder.WrittenSpan.SequenceEqual(name.AsSpan());
    }

    public static bool HasOrInheritsAttributeWithFullyQualifiedMetadataName(
        this ITypeSymbol typeSymbol,
        string name
    )
    {
        for (
            var currentType = typeSymbol;
            currentType is not null;
            currentType = currentType.BaseType
        )
            if (currentType.HasAttributeWithFullyQualifiedMetadataName(name))
                return true;

        return false;
    }

    public static bool HasAttributeWithFullyQualifiedMetadataName(this ISymbol symbol, string name)
    {
        return symbol
            .GetAttributes()
            .Any(attribute =>
                attribute.AttributeClass?.HasFullyQualifiedMetadataName(name) == true
            );
    }

    public static bool InheritsFromFullyQualifiedMetadataName(
        this ITypeSymbol typeSymbol,
        string name
    )
    {
        var baseType = typeSymbol.BaseType;

        while (baseType is not null)
        {
            if (baseType.HasFullyQualifiedMetadataName(name))
                return true;

            baseType = baseType.BaseType;
        }

        return false;
    }

    public static bool ImplementsFromFullyQualifiedMetadataName(
        this ITypeSymbol typeSymbol,
        string name
    )
    {
        ImmutableArray<INamedTypeSymbol>? baseInterfaces = typeSymbol.AllInterfaces;

        foreach (var interfaceType in baseInterfaces)
            if (interfaceType.HasFullyQualifiedMetadataName(name))
                return true;

        return false;
    }

    public static string GetFullyQualifiedNameWithNullabilityAnnotations(this ISymbol symbol)
    {
        return symbol.ToDisplayString(
            SymbolDisplayFormat.FullyQualifiedFormat.AddMiscellaneousOptions(
                SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
            )
        );
    }

    public static Accessibility GetEffectiveAccessibility(this ISymbol symbol)
    {
        // Start by assuming it's visible
        var visibility = Accessibility.Public;

        // Handle special cases
        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (symbol.Kind)
        {
            case SymbolKind.Alias:
                return Accessibility.Private;
            case SymbolKind.Parameter:
                return symbol.ContainingSymbol.GetEffectiveAccessibility();
            case SymbolKind.TypeParameter:
                return Accessibility.Private;
        }

        // Traverse the symbol hierarchy to determine the effective accessibility
        while (symbol is not null && symbol.Kind != SymbolKind.Namespace)
        {
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (symbol.DeclaredAccessibility)
            {
                case Accessibility.NotApplicable:
                case Accessibility.Private:
                    return Accessibility.Private;
                case Accessibility.Internal:
                case Accessibility.ProtectedAndInternal:
                    visibility = Accessibility.Internal;
                    break;
            }

            symbol = symbol.ContainingSymbol;
        }

        return visibility;
    }

    public static bool CanBeAccessedFrom(this ISymbol symbol, IAssemblySymbol assembly)
    {
        var accessibility = symbol.GetEffectiveAccessibility();

        return accessibility == Accessibility.Public
            || (
                accessibility == Accessibility.Internal
                && symbol.ContainingAssembly.GivesAccessTo(assembly)
            );
    }

    private static void AppendFullyQualifiedMetadataName(
        this ITypeSymbol symbol,
        in ImmutableArrayBuilder<char> builder
    )
    {
        BuildFrom(symbol, in builder);
        return;

        static void BuildFrom(ISymbol? symbol, in ImmutableArrayBuilder<char> builder)
        {
            switch (symbol)
            {
                // Namespaces that are nested also append a leading '.'
                case INamespaceSymbol { ContainingNamespace.IsGlobalNamespace: false }:
                    BuildFrom(symbol.ContainingNamespace, in builder);
                    builder.Add('.');
                    builder.AddRange(symbol.MetadataName.AsSpan());
                    break;

                // Other namespaces (ie: the one right before global) skip the leading '.'
                case INamespaceSymbol { IsGlobalNamespace: false }:
                // Types with no namespace just have their metadata name directly written
                case ITypeSymbol { ContainingSymbol: INamespaceSymbol { IsGlobalNamespace: true } }:
                    builder.AddRange(symbol.MetadataName.AsSpan());
                    break;

                // Types with a containing non-global namespace also append a leading '.'
                case ITypeSymbol { ContainingSymbol: INamespaceSymbol namespaceSymbol }:
                    BuildFrom(namespaceSymbol, in builder);
                    builder.Add('.');
                    builder.AddRange(symbol.MetadataName.AsSpan());
                    break;

                // Nested types append a leading '+'
                case ITypeSymbol { ContainingSymbol: ITypeSymbol typeSymbol }:
                    BuildFrom(typeSymbol, in builder);
                    builder.Add('+');
                    builder.AddRange(symbol.MetadataName.AsSpan());
                    break;
            }
        }
    }
}
