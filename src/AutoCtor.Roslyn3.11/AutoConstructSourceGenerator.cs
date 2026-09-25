using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace AutoCtor;

public sealed partial class AutoConstructSourceGenerator : ISourceGenerator
{
    private sealed class SyntaxContextReceiver(CancellationToken cancellationToken) : ISyntaxContextReceiver
    {
        public List<TypeModel>? TypeModels { get; private set; }
        public List<PostCtorModel>? MarkedMethods { get; private set; }
        public List<ServiceProviderModel>? ServiceProviders { get; private set; }

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            INamedTypeSymbol? type;
            IMethodSymbol? method;
            if (GeneratorUtilities.IsTypeDeclarationWithAttributes(context.Node, cancellationToken)

                && (type = GeneratorUtilities.GetPrimarySymbol<INamedTypeSymbol>(context, cancellationToken)) != null)
            {
                if (Utilities.HasAttribute(type, AttributeNames.AutoConstruct))
                    (TypeModels ??= []).Add(TypeModel.Create(type));

                if (Utilities.HasAttribute(type, AttributeNames.ServiceProvider))
                    (ServiceProviders ??= []).Add(ServiceProviderModel.Create(type));
            }

            else if (GeneratorUtilities.IsMethodDeclarationWithAttributes(context.Node, cancellationToken)

                && (method = GeneratorUtilities.GetSymbol<IMethodSymbol>(context, cancellationToken)) != null

                && Utilities.HasAttribute(method, AttributeNames.AutoPostConstruct))
            {
                (MarkedMethods ??= []).Add(PostCtorModel.Create(method));
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
            return;

        var enableGuards = false;
        if (context.AnalyzerConfigOptions.GlobalOptions
            .TryGetValue("build_property.AutoCtorGuards", out var projectGuardSetting))
        {
            enableGuards =
                projectGuardSetting.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                projectGuardSetting.Equals("enable", StringComparison.OrdinalIgnoreCase);
        }

        Emitter.GenerateSource(context, (
            receiver.TypeModels?.ToImmutableArray() ?? ImmutableArray<TypeModel>.Empty,
            receiver.MarkedMethods?.ToImmutableArray() ?? ImmutableArray<PostCtorModel>.Empty,
            enableGuards,
            receiver.ServiceProviders?.ToImmutableArray() ?? ImmutableArray<ServiceProviderModel>.Empty,
            DuckTypes.Create(context.Compilation)));
    }
}
