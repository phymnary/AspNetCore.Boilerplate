using System.Collections.Immutable;
using AspNetCore.Boilerplate.Roslyn.Extensions;
using AspNetCore.Boilerplate.Roslyn.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AspNetCore.Boilerplate.Roslyn.Components.Service;

public partial class ServiceGenerator
{
    private static class Execute
    {
        public static bool TryGetDependencyInfo(
            INamedTypeSymbol typeSymbol,
            ImmutableArray<AttributeData> matchedAttributes,
            CancellationToken token,
            out ServiceInfo? dependencyInfo
        )
        {
            if (typeSymbol is not { IsAbstract: false, IsGenericType: false })
            {
                dependencyInfo = null;
                return false;
            }

            string serviceTypeName;
            var typeName = typeSymbol.GetFullyQualifiedName();
            var attribute = matchedAttributes.First();

            if (attribute.GetNamedArgument("IsSelf") is true)
                serviceTypeName = typeName;
            else if (typeSymbol.Interfaces.Length > 0)
                serviceTypeName = typeSymbol.Interfaces[0].GetFullyQualifiedName();
            else if (typeSymbol.BaseType is { Interfaces.Length: > 0 } baseType)
                serviceTypeName = baseType.Interfaces[0].GetFullyQualifiedName();
            else
                serviceTypeName = typeName;

            if (attribute.ConstructorArguments.Length == 0)
            {
                dependencyInfo = null;
                return false;
            }

            token.ThrowIfCancellationRequested();

            var lifetime = attribute.ConstructorArguments.FirstOrDefault();
            var fullyQualifiedLifetime = lifetime.ToCSharpString();

            dependencyInfo = new ServiceInfo(
                serviceTypeName,
                typeName,
                fullyQualifiedLifetime.Substring(fullyQualifiedLifetime.LastIndexOf('.') + 1)
            );

            return true;
        }

        public static bool TryGetModuleHierarchy(
            ClassDeclarationSyntax nodeSyntax,
            INamedTypeSymbol symbol,
            CancellationToken _,
            out HierarchyInfo? hierarchyInfo
        )
        {
            if (!nodeSyntax.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                hierarchyInfo = null;
                return false;
            }

            hierarchyInfo = HierarchyInfo.From(symbol);
            return true;
        }

        public static ExpressionStatementSyntax GetRegistrationExpression(ServiceInfo info)
        {
            return ExpressionStatement(
                InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName(
                                "global::Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions"
                            ),
                            GenericName(Identifier($"Add{info.Lifetime}"))
                                .WithTypeArgumentList(
                                    TypeArgumentList(
                                        SeparatedList<TypeSyntax>(
                                            new SyntaxNodeOrToken[]
                                            {
                                                IdentifierName(info.ServiceTypeName),
                                                Token(SyntaxKind.CommaToken),
                                                IdentifierName(info.ImplementationTypeName),
                                            }
                                        )
                                    )
                                )
                        )
                    )
                    .WithArgumentList(
                        ArgumentList(SingletonSeparatedList(Argument(IdentifierName("services"))))
                    )
            );
        }
    }
}
