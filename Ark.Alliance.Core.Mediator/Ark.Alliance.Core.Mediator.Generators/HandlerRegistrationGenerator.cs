using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace Ark.Alliance.Core.Mediator.Generators;

/// <summary>
/// Source generator that discovers handler implementations and emits
/// dependency injection registrations.
/// </summary>
[Generator]
public sealed class HandlerRegistrationGenerator : UniversalIncrementalGenerator<ClassDeclarationSyntax, HandlerRegistration>
{
    #region Overrides

    protected override string GeneratorName => "HandlerRegistration";

    /// <summary>
    /// Gathers candidate class declarations that implement one of the handler
    /// interfaces.
    /// </summary>
    protected override IncrementalValuesProvider<ClassDeclarationSyntax> CollectInputs(IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => node is ClassDeclarationSyntax cls && cls.BaseList != null,
            static (ctx, _) => (ClassDeclarationSyntax)ctx.Node);
    }

    /// <summary>
    /// Converts class declarations into their interface/implementation pairs.
    /// </summary>
    protected override GenerationInput<HandlerRegistration> ProcessInputs(
        ImmutableArray<ClassDeclarationSyntax> inputs,
        Compilation compilation,
        CancellationToken cancellationToken)
    {
        var regs = inputs
            .SelectMany(cls => GetRegistrations(cls, compilation, cancellationToken))
            .Distinct()
            .ToImmutableArray();

        var hash = ComputeIncrementalHash(regs.Select(r => $"{r.Interface}:{r.Implementation}"));
        var meta = new Dictionary<string, string>
        {
            ["HandlerCount"] = regs.Length.ToString(),
            ["Assembly"] = compilation.AssemblyName ?? string.Empty
        };

        return new GenerationInput<HandlerRegistration>(regs, hash, meta);
    }

    /// <summary>
    /// Emits the registration extension class.
    /// </summary>
    protected override IEnumerable<GenerationOutput> GenerateCode(GenerationInput<HandlerRegistration> input)
    {
#if DEBUG
        Debug.WriteLine($"[HandlerRegistration] Generating for {input.Items.Length} handlers");
#endif
        var sb = new StringBuilder();
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine();
        sb.AppendLine("namespace Ark.Alliance.Core.Mediator.Messaging;");
        sb.AppendLine();
        sb.AppendLine("public static partial class IServiceCollectionExtensions");
        sb.AppendLine("{");
        sb.AppendLine("    static partial void RegisterGeneratedHandlers(IServiceCollection services)");
        sb.AppendLine("    {");

        foreach (var reg in input.Items)
            sb.AppendLine($"        services.AddTransient<{reg.Interface}, {reg.Implementation}>();");

        sb.AppendLine("    }");
        sb.AppendLine("}");

        yield return new GenerationOutput("CqrsHandlers.g.cs", sb.ToString());
    }

    #endregion Overrides

    #region Helpers
    /// <summary>
    /// Extracts interface/implementation tuples for a class declaration.
    /// </summary>
    private static IEnumerable<HandlerRegistration> GetRegistrations(
        ClassDeclarationSyntax cls,
        Compilation compilation,
        CancellationToken ct)
    {
        var model = compilation.GetSemanticModel(cls.SyntaxTree);
        if (model.GetDeclaredSymbol(cls, ct) is not INamedTypeSymbol symbol)
            yield break;

        foreach (var iface in symbol.AllInterfaces)
        {
            if (!iface.IsGenericType)
                continue;

            var def = iface.ConstructedFrom.ToDisplayString();
            if (def is not (
                "Ark.Alliance.Core.Mediator.Messaging.ICommandHandler<TCommand, TResult>" or
                "Ark.Alliance.Core.Mediator.Messaging.IQueryHandler<TQuery, TResult>" or
                "Ark.Alliance.Core.Mediator.Messaging.IEventHandler<TEvent>" or
                "Ark.Alliance.Core.Mediator.Messaging.IStreamRequestHandler<TRequest, T>"))
                continue;

            var ifaceName = iface.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var implName = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            yield return new HandlerRegistration(ifaceName, implName);
        }
    }

    #endregion Helpers
}
