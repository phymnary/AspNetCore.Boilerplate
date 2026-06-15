using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Phymnary.SugarPot.DependencyInjection.Roslyn.Constants;
using Phymnary.SugarPot.RoslynComponents.Helpers;
using Phymnary.SugarPot.RoslynComponents.Models;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Phymnary.SugarPot.DependencyInjection.Roslyn.Components.AutoRegister;

partial class AutoRegisterGenerator
{
    private static class BuildSyntax
    {
        public static CompilationUnitSyntax GetCompilationUnitForDependency(
            HierarchyInfo hierarchyInfo,
            ImmutableArray<ExpressionStatementSyntax> registerExpressions
        )
        {
            using ImmutableArrayBuilder<MemberDeclarationSyntax> methodSyntaxesBuilder =
                ImmutableArrayBuilder<MemberDeclarationSyntax>.Rent();

            methodSyntaxesBuilder.Add(
                MethodDeclaration(
                        PredefinedType(Token(SyntaxKind.VoidKeyword)),
                        Identifier("AddDependencies")
                    )
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithParameterList(
                        ParameterList(
                            SingletonSeparatedList(
                                Parameter(Identifier("services"))
                                    .WithType(
                                        IdentifierName(
                                            "global::Microsoft.Extensions.DependencyInjection.IServiceCollection"
                                        )
                                    )
                            )
                        )
                    )
                    .AddBodyStatements([.. registerExpressions])
            );

            CompilationUnitSyntax typeDeclarationSyntax = hierarchyInfo.GetCompilationUnit(
                methodSyntaxesBuilder.ToImmutable(),
                BaseList(
                    SingletonSeparatedList<BaseTypeSyntax>(
                        SimpleBaseType(
                            IdentifierName(
                                "global::" + GeneratorConstant.LibNamespace + ".IAutoRegister"
                            )
                        )
                    )
                )
            );

            return typeDeclarationSyntax;
        }
    }
}
