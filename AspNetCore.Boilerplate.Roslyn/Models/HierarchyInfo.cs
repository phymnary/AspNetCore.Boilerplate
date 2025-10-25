using AspNetCore.Boilerplate.Roslyn.Extensions;
using AspNetCore.Boilerplate.Roslyn.Helper;
using Microsoft.CodeAnalysis;
using static Microsoft.CodeAnalysis.SymbolDisplayTypeQualificationStyle;

namespace AspNetCore.Boilerplate.Roslyn.Models;

public sealed record HierarchyInfo(
    string FilenameHint,
    string MetadataName,
    string Namespace,
    EquatableArray<TypeInfo> Hierarchy
)
{
    /// <summary>
    ///     Creates a new <see cref="HierarchyInfo" /> instance from a given <see cref="INamedTypeSymbol" />.
    /// </summary>
    /// <param name="typeSymbol">The input <see cref="INamedTypeSymbol" /> instance to gather info for.</param>
    /// <returns>A <see cref="HierarchyInfo" /> instance describing <paramref name="typeSymbol" />.</returns>
    public static HierarchyInfo From(INamedTypeSymbol typeSymbol)
    {
        using var hierarchy = ImmutableArrayBuilder<TypeInfo>.Rent();

        for (var parent = typeSymbol; parent is not null; parent = parent.ContainingType)
            hierarchy.Add(
                new TypeInfo(
                    parent.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                    parent.TypeKind,
                    parent.IsRecord
                )
            );

        return new HierarchyInfo(
            typeSymbol.GetFullyQualifiedMetadataName(),
            typeSymbol.MetadataName,
            typeSymbol.ContainingNamespace.ToDisplayString(
                new SymbolDisplayFormat(typeQualificationStyle: NameAndContainingTypesAndNamespaces)
            ),
            hierarchy.ToImmutable()
        );
    }

    public string FilenameHint { get; } = FilenameHint;
    public string MetadataName { get; } = MetadataName;
    public string Namespace { get; } = Namespace;
    public EquatableArray<TypeInfo> Hierarchy { get; } = Hierarchy;
}
