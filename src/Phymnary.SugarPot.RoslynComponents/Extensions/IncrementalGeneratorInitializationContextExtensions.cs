using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Phymnary.SugarPot.RoslynComponents.Extensions;

public readonly struct GeneratorAttributeSyntaxContextWithOptions(
    GeneratorAttributeSyntaxContext syntaxContext,
    AnalyzerConfigOptions globalOptions
)
{
    /// <inheritdoc cref="GeneratorAttributeSyntaxContext.TargetNode"/>
    public SyntaxNode TargetNode { get; } = syntaxContext.TargetNode;

    /// <inheritdoc cref="GeneratorAttributeSyntaxContext.TargetSymbol"/>
    public ISymbol TargetSymbol { get; } = syntaxContext.TargetSymbol;

    /// <inheritdoc cref="GeneratorAttributeSyntaxContext.SemanticModel"/>
    public SemanticModel SemanticModel { get; } = syntaxContext.SemanticModel;

    /// <inheritdoc cref="GeneratorAttributeSyntaxContext.Attributes"/>
    public ImmutableArray<AttributeData> Attributes { get; } = syntaxContext.Attributes;

    /// <inheritdoc cref="AnalyzerConfigOptionsProvider.GlobalOptions"/>
    public AnalyzerConfigOptions GlobalOptions { get; } = globalOptions;
}

public static class IncrementalGeneratorInitializationContextExtensions
{
    public static IncrementalValuesProvider<T> ForAttributeWithMetadataNameAndOptions<T>(
        this IncrementalGeneratorInitializationContext context,
        string fullyQualifiedMetadataName,
        Func<SyntaxNode, CancellationToken, bool> predicate,
        Func<GeneratorAttributeSyntaxContextWithOptions, CancellationToken, T> transform
    )
    {
        // Invoke 'ForAttributeWithMetadataName' normally, but just return the context directly
        IncrementalValuesProvider<GeneratorAttributeSyntaxContext> syntaxContext =
            context.SyntaxProvider.ForAttributeWithMetadataName(
                fullyQualifiedMetadataName,
                predicate,
                static (context, token) => context
            );

        // Do the same for the analyzer config options
        IncrementalValueProvider<AnalyzerConfigOptions> configOptions =
            context.AnalyzerConfigOptionsProvider.Select(
                static (provider, token) => provider.GlobalOptions
            );

        // Merge the two and invoke the provided transform on these two values. Neither value
        // is equatable, meaning the pipeline will always re-run until this point. This is
        // intentional: we don't want any symbols or other expensive objects to be kept alive
        // across incremental steps, especially if they could cause entire compilations to be
        // rooted, which would significantly increase memory use and introduce more GC pauses.
        // In this specific case, flowing non equatable values in a pipeline is therefore fine.
        return syntaxContext
            .Combine(configOptions)
            .Select(
                (input, token) =>
                    transform(
                        new GeneratorAttributeSyntaxContextWithOptions(input.Left, input.Right),
                        token
                    )
            );
    }

    /// <summary>
    /// Conditionally invokes <see cref="IncrementalGeneratorInitializationContext.RegisterSourceOutput{TSource}(IncrementalValueProvider{TSource}, Action{SourceProductionContext, TSource})"/>
    /// if the value produced by the input <see cref="IncrementalValueProvider{TValue}"/> is <see langword="true"/>.
    /// </summary>
    /// <param name="context">The input <see cref="IncrementalGeneratorInitializationContext"/> value being used.</param>
    /// <param name="source">The source <see cref="IncrementalValueProvider{TValues}"/> instance.</param>
    /// <param name="action">The conditional <see cref="Action"/> to invoke.</param>
    public static void RegisterConditionalSourceOutput(
        this IncrementalGeneratorInitializationContext context,
        IncrementalValueProvider<bool> source,
        Action<SourceProductionContext> action
    )
    {
        context.RegisterSourceOutput(
            source,
            (context, condition) =>
            {
                if (condition)
                {
                    action(context);
                }
            }
        );
    }

    /// <summary>
    /// Conditionally invokes <see cref="IncrementalGeneratorInitializationContext.RegisterImplementationSourceOutput{TSource}(IncrementalValueProvider{TSource}, Action{SourceProductionContext, TSource})"/>
    /// if the value produced by the input <see cref="IncrementalValueProvider{TValue}"/> is <see langword="true"/>, and also supplying a given input state.
    /// </summary>
    /// <typeparam name="T">The type of state to pass to the source production callback to invoke.</typeparam>
    /// <param name="context">The input <see cref="IncrementalGeneratorInitializationContext"/> value being used.</param>
    /// <param name="source">The source <see cref="IncrementalValueProvider{TValues}"/> instance.</param>
    /// <param name="action">The conditional <see cref="Action{T}"/> to invoke.</param>
    public static void RegisterConditionalImplementationSourceOutput<T>(
        this IncrementalGeneratorInitializationContext context,
        IncrementalValueProvider<(bool Condition, T State)> source,
        Action<SourceProductionContext, T> action
    )
    {
        context.RegisterImplementationSourceOutput(
            source,
            (context, item) =>
            {
                if (item.Condition)
                {
                    action(context, item.State);
                }
            }
        );
    }

    /// <summary>
    /// Registers an output node into an <see cref="IncrementalGeneratorInitializationContext"/> to output diagnostics.
    /// </summary>
    /// <param name="context">The input <see cref="IncrementalGeneratorInitializationContext"/> instance.</param>
    /// <param name="diagnostics">The input <see cref="IncrementalValuesProvider{TValues}"/> sequence of diagnostics.</param>
    public static void ReportDiagnostics(
        this IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<ImmutableArray<Diagnostic>> diagnostics
    )
    {
        context.RegisterSourceOutput(
            diagnostics,
            static (context, diagnostics) =>
            {
                foreach (Diagnostic diagnostic in diagnostics)
                {
                    context.ReportDiagnostic(diagnostic);
                }
            }
        );
    }

    /// <summary>
    /// Registers an output node into an <see cref="IncrementalGeneratorInitializationContext"/> to output diagnostics.
    /// </summary>
    /// <param name="context">The input <see cref="IncrementalGeneratorInitializationContext"/> instance.</param>
    /// <param name="diagnostics">The input <see cref="IncrementalValuesProvider{TValues}"/> sequence of diagnostics.</param>
    public static void ReportDiagnostics(
        this IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<Diagnostic> diagnostics
    )
    {
        context.RegisterSourceOutput(
            diagnostics,
            static (context, diagnostic) => context.ReportDiagnostic(diagnostic)
        );
    }
}
