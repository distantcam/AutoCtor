using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace AutoCtor;

public sealed partial class AutoConstructSourceGenerator : ISourceGenerator
{
    private sealed class SyntaxContextReceiver(CancellationToken cancellationToken) : ISyntaxContextReceiver
    {
        public List<TypeModel>? TypeModels { get; private set; }
        public List<ServiceProviderModel>? ServiceProviders { get; private set; }

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            INamedTypeSymbol? type;
            if (GeneratorUtilities.IsTypeDeclarationWithAttributes(context.Node, cancellationToken)

                && (type = GeneratorUtilities.GetPrimarySymbol<INamedTypeSymbol>(context, cancellationToken)) != null)
            {
                if (Utilities.HasAttribute(type, AttributeNames.AutoConstructName))
                    (TypeModels ??= []).Add(TypeModel.Create(type));

                if (Utilities.HasAttribute(type, AttributeNames.ServiceProviderName))
                    (ServiceProviders ??= []).Add(ServiceProviderModel.Create(type));
            }
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(static () =>
            new SyntaxContextReceiver(CancellationToken.None));
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not SyntaxContextReceiver receiver
            || (receiver.TypeModels == null && receiver.ServiceProviders == null))
        {
            return;
        }

        var enableGuards = false;
        if (context.AnalyzerConfigOptions.GlobalOptions
            .TryGetValue("build_property.AutoCtorGuards", out var projectGuardSetting))
        {
            enableGuards =
                projectGuardSetting.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                projectGuardSetting.Equals("enable", StringComparison.OrdinalIgnoreCase);
        }

        var types = receiver.TypeModels?.ToImmutableArray() ?? ImmutableArray<TypeModel>.Empty;
        foreach (var type in types)
            Emitter.GenerateSource(context, (type, enableGuards));

        IoCEmitter.GenerateProviders(context, (
            receiver.ServiceProviders?.ToImmutableArray() ?? ImmutableArray<ServiceProviderModel>.Empty,
            types,
            DuckTypes.Create(context.Compilation)));
    }
}
