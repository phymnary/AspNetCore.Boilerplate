using System.Collections.Immutable;
using AspNetCore.Boilerplate.Roslyn.Constants;
using AspNetCore.Boilerplate.Roslyn.Extensions;
using AspNetCore.Boilerplate.Roslyn.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AspNetCore.Boilerplate.Roslyn.Components.Endpoint;

[Generator(LanguageNames.CSharp)]
public partial class EndpointGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<GroupConfigRouteMethodInfo> groupPatternInfos = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                $"{GeneratorConstant.LibNamespaceApi}.RoutePatternAttribute",
                static (node, _) =>
                    node is MethodDeclarationSyntax { Parent: ClassDeclarationSyntax },
                static (ctx, _) =>
                    Execute.TryGetGroupConfigRouteMethodInfo(
                        (IMethodSymbol)ctx.TargetSymbol,
                        out var info
                    )
                        ? info
                        : null
            )
            .Where(it => it is not null)!;

        IncrementalValuesProvider<GroupConfigRouteMethodInfo> groupBuilderInfos = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                $"{GeneratorConstant.LibNamespaceApi}.RouteBuilderAttribute",
                static (node, _) =>
                    node is MethodDeclarationSyntax { Parent: ClassDeclarationSyntax },
                static (ctx, _) =>
                {
                    Execute.TryGetGroupConfigRouteMethodInfo(
                        (IMethodSymbol)ctx.TargetSymbol,
                        out var info
                    );

                    return info;
                }
            )
            .Where(it => it is not null)!;

        IncrementalValuesProvider<EndpointInfo> endpointHierarchies = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                $"{GeneratorConstant.LibNamespaceApi}.EndpointAttribute",
                static (node, _) => node is ClassDeclarationSyntax,
                static (ctx, token) =>
                {
                    var typeSymbol = (INamedTypeSymbol)ctx.TargetSymbol;

                    if (ctx.Attributes.FirstOrDefault() is not { } attributeData)
                        return null;

                    return Execute.TryGetEndpointInfo(
                        typeSymbol,
                        attributeData,
                        token,
                        out var info
                    )
                        ? info
                        : null;
                }
            )
            .Where(it => it is not null)!;

        IncrementalValuesProvider<ControllerInfo> controllerHierarchies =
            context.SyntaxProvider.ForAttributeWithMetadataName(
                $"{GeneratorConstant.LibNamespaceApi}.ApiSchemaAttribute",
                static (node, _) => node is ClassDeclarationSyntax,
                static (ctx, _) =>
                {
                    var classSymbol = (INamedTypeSymbol)ctx.TargetSymbol;

                    return new ControllerInfo(
                        HierarchyInfo.From(classSymbol),
                        classSymbol.IsStatic
                    );
                }
            );

        IncrementalValuesProvider<(
            (EndpointInfo Endpoint, ImmutableArray<GroupConfigRouteMethodInfo> Patterns) Item,
            ImmutableArray<GroupConfigRouteMethodInfo> Builder
        )> endpointsWithPatterns = endpointHierarchies
            .Combine(groupPatternInfos.Collect())
            .Combine(groupBuilderInfos.Collect());

        context.RegisterSourceOutput(
            endpointsWithPatterns,
            static (src, item) =>
            {
                var ((endpoint, patterns), builders) = item;

                var closestPattern =
                    endpoint.RoutePatternPropertyName != string.Empty
                        ? null
                        : Execute.FindClosestConfigInfo(endpoint, patterns);

                var methodExpression = Execute.GetMapRouteMethodExpression(
                    endpoint,
                    closestPattern
                );

                if (methodExpression is null)
                    return;

                var buildRouteExpression = Execute.GetBuildRouteMethodExpression(
                    endpoint,
                    Execute.FindClosestConfigInfo(endpoint, builders)
                );

                var compilationUnit = BuildSyntax.GetCompilationUnitForEndpoint(
                    endpoint.Hierarchy,
                    methodExpression,
                    buildRouteExpression
                );

                src.AddSource($"{endpoint.Hierarchy.FilenameHint}.g.cs", compilationUnit);
            }
        );

        IncrementalValuesProvider<(
            ControllerInfo Controller,
            ImmutableArray<EndpointInfo> Endpoints
        )> controllerWithEndpoints = controllerHierarchies.Combine(endpointHierarchies.Collect());

        context.RegisterSourceOutput(
            controllerWithEndpoints,
            static (src, item) =>
            {
                var nonConstructExpression = item
                    .Endpoints.Where(endpoint => !endpoint.HasConstructor)
                    .Select(Execute.GetMapEndpointExpression)
                    .ToImmutableArray();

                var hasConstructExpression = item
                    .Endpoints.Where(endpoint => endpoint.HasConstructor)
                    .Select(Execute.GetMapConstructedEndpointExpression)
                    .ToImmutableArray();

                var compilationUnit = BuildSyntax.GetCompilationUnitForController(
                    item.Controller.Hierarchy,
                    item.Controller.IsStatic,
                    nonConstructExpression,
                    hasConstructExpression
                );

                src.AddSource($"{item.Controller.Hierarchy.FilenameHint}.g.cs", compilationUnit);
            }
        );
    }
}
