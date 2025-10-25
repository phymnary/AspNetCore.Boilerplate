using System.Collections.Immutable;
using AspNetCore.Boilerplate.Roslyn.Constants;
using AspNetCore.Boilerplate.Roslyn.Extensions;
using AspNetCore.Boilerplate.Roslyn.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AspNetCore.Boilerplate.Roslyn.Components.Endpoint;

public partial class EndpointGenerator
{
    private static readonly string[] Methods =
    {
        "Get",
        "Put",
        "Post",
        "Delete",
        "Head",
        "Options",
        "Trace",
        "Patch",
        "Connect",
    };

    private static readonly string[] CompareMethods = Methods
        .Select(it => "." + it.ToLower())
        .ToArray();

    private static class Execute
    {
        public static bool TryGetGroupConfigRouteMethodInfo(
            IMethodSymbol symbol,
            out GroupConfigRouteMethodInfo? info
        )
        {
            if (!symbol.IsStatic)
            {
                info = null!;
                return false;
            }

            var containingType = symbol.ContainingType;

            if (
                containingType.HasAttributeWithFullyQualifiedMetadataName(
                    $"{GeneratorConstant.LibNamespaceApi}.EndpointAttribute"
                )
            )
            {
                info = null;
                return false;
            }

            var namespaces = containingType
                .ContainingNamespace.ToDisplayString(
                    new SymbolDisplayFormat(
                        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces
                    )
                )
                .Split('.')
                .ToImmutableArray();

            info = new GroupConfigRouteMethodInfo(
                containingType.GetFullyQualifiedName(),
                symbol.Name,
                namespaces
            );

            return true;
        }

        public static bool TryGetEndpointInfo(
            INamedTypeSymbol symbol,
            AttributeData attribute,
            CancellationToken token,
            out EndpointInfo? info
        )
        {
            string methodName;
            var hierarchy = HierarchyInfo.From(symbol);

            token.ThrowIfCancellationRequested();

            if (attribute.ConstructorArguments.Length > 0)
            {
                methodName = attribute.ConstructorArguments[0].ToCSharpString();
                methodName = methodName.Substring(methodName.LastIndexOf('.') + 1);
            }
            else if (
                Methods.FirstOrDefault(it => symbol.Name.StartsWith(it)) is { } methodInTypeName
            )
            {
                methodName = methodInTypeName;
            }
            else if (
                hierarchy.Namespace.ToLower() is { } lowerNs
                && CompareMethods.FirstOrDefault(it => lowerNs.EndsWith(it))
                    is { } methodInNamespace
            )
            {
                methodName = Methods[Array.IndexOf(CompareMethods, methodInNamespace)];
            }
            else
            {
                info = null;
                return false;
            }

            token.ThrowIfCancellationRequested();

            var members = symbol.GetMembers();
            var hasRoutePatternProperty = members.Any(member =>
                member is IPropertySymbol { Name: "RoutePattern" }
            );

            var hasMethod = members.Any(member =>
                member is IMethodSymbol { Name: "BuildRoute" } methodSymbol
                && methodSymbol.ReturnType.GetFullyQualifiedName()
                    == "global::Microsoft.AspNetCore.Builder.RouteHandlerBuilder"
                && methodSymbol.Parameters.Length == 1
                && methodSymbol.Parameters.FirstOrDefault()?.Type.GetFullyQualifiedName()
                    == "global::Microsoft.AspNetCore.Builder.RouteHandlerBuilder"
            );

            token.ThrowIfCancellationRequested();

            info = new EndpointInfo(
                symbol.GetFullyQualifiedName(),
                methodName,
                "HandleAsync",
                hasRoutePatternProperty ? "RoutePattern" : string.Empty,
                hasMethod ? "BuildRoute" : string.Empty,
                symbol.Constructors.Any(constructor => constructor.Parameters.Length > 0),
                hierarchy.Namespace.Split('.').ToImmutableArray(),
                hierarchy
            );

            token.ThrowIfCancellationRequested();

            return true;
        }

        public static GroupConfigRouteMethodInfo? FindClosestConfigInfo(
            EndpointInfo endpoint,
            ImmutableArray<GroupConfigRouteMethodInfo> methodInfos
        )
        {
            GroupConfigRouteMethodInfo? selected = null;
            var highest = -1;

            foreach (var methodInfo in methodInfos)
            {
                int i;
                for (i = 0; i < methodInfo.Namespaces.Length && i < endpoint.Namespaces.Length; i++)
                    if (methodInfo.Namespaces[i] != endpoint.Namespaces[i])
                        break;

                if (
                    i <= highest
                    && (i != highest || methodInfo.Namespaces.Length >= selected!.Namespaces.Length)
                )
                    continue;

                highest = i;
                selected = methodInfo;
            }

            return selected;
        }

        public static LocalDeclarationStatementSyntax? GetMapRouteMethodExpression(
            EndpointInfo info,
            GroupConfigRouteMethodInfo? patternInfo
        )
        {
            ArgumentSyntax argument;
            if (info.RoutePatternPropertyName != string.Empty)
                argument = Argument(IdentifierName("RoutePattern"));
            else if (patternInfo is not null)
                argument = Argument(
                    InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName(patternInfo.ContainingType),
                                IdentifierName(patternInfo.MethodName)
                            )
                        )
                        .WithArgumentList(
                            ArgumentList(SingletonSeparatedList(Argument(ThisExpression())))
                        )
                );
            else
                return null;

            var invocation = InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(
                            "global::Microsoft.AspNetCore.Builder.EndpointRouteBuilderExtensions"
                        ),
                        IdentifierName($"Map{info.HttpMethod}")
                    )
                )
                .WithArgumentList(
                    ArgumentList(
                        SeparatedList<ArgumentSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                Argument(IdentifierName("app")),
                                Token(SyntaxKind.CommaToken),
                                argument,
                                Token(SyntaxKind.CommaToken),
                                Argument(IdentifierName(info.HandleMethodName)),
                            }
                        )
                    )
                );

            return LocalDeclarationStatement(
                VariableDeclaration(IdentifierName("var"))
                    .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(Identifier("builder"))
                                .WithInitializer(EqualsValueClause(invocation))
                        )
                    )
            );
        }

        public static ExpressionStatementSyntax? GetBuildRouteMethodExpression(
            EndpointInfo info,
            GroupConfigRouteMethodInfo? builderInfo
        )
        {
            InvocationExpressionSyntax invocation;
            if (info.BuildRouteMethodName != string.Empty)
                invocation = InvocationExpression(IdentifierName(info.BuildRouteMethodName));
            else if (builderInfo is not null)
                invocation = InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(builderInfo.ContainingType),
                        IdentifierName(builderInfo.MethodName)
                    )
                );
            else
                return null;

            return ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName("builder"),
                    invocation.WithArgumentList(
                        ArgumentList(SingletonSeparatedList(Argument(IdentifierName("builder"))))
                    )
                )
            );
        }

        public static ExpressionStatementSyntax GetMapEndpointExpression(EndpointInfo endpoint)
        {
            return ExpressionStatement(
                InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName(
                                $"{GeneratorConstant.Global}AspNetCore.Boilerplate.Api.Extensions.EndpointRouteBuilderExtensions"
                            ),
                            GenericName("MapEndpoint")
                                .WithTypeArgumentList(
                                    TypeArgumentList(
                                        SingletonSeparatedList<TypeSyntax>(
                                            IdentifierName(endpoint.TypeName)
                                        )
                                    )
                                )
                        )
                    )
                    .WithArgumentList(
                        ArgumentList(SingletonSeparatedList(Argument(IdentifierName("app"))))
                    )
            );
        }

        public static ExpressionStatementSyntax GetMapConstructedEndpointExpression(
            EndpointInfo endpoint
        )
        {
            return ExpressionStatement(
                InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName(
                                $"{GeneratorConstant.Global}AspNetCore.Boilerplate.Api.Extensions.EndpointRouteBuilderExtensions"
                            ),
                            GenericName("MapEndpoint")
                                .WithTypeArgumentList(
                                    TypeArgumentList(
                                        SingletonSeparatedList<TypeSyntax>(
                                            IdentifierName(endpoint.TypeName)
                                        )
                                    )
                                )
                        )
                    )
                    .WithArgumentList(
                        ArgumentList(
                            SeparatedList<ArgumentSyntax>(
                                new SyntaxNodeOrToken[]
                                {
                                    Argument(IdentifierName("app")),
                                    Token(SyntaxKind.CommaToken),
                                    Argument(IdentifierName("serviceProvider")),
                                }
                            )
                        )
                    )
            );
        }
    }
}
