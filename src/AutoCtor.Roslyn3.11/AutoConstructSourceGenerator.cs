using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoCtor;

public sealed partial class AttributeSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization(static c =>
            c.AddSource(Emitter.HintName, Emitter.GenerateSource()));
    }
    public void Execute(GeneratorExecutionContext context) { }
}

public sealed partial class AutoConstructSourceGenerator : ISourceGenerator
{
    private sealed class SyntaxContextReceiver(CancellationToken cancellationToken) : ISyntaxContextReceiver
    {
        public List<TypeModel>? TypeModels { get; private set; }
        public List<PostCtorModel>? MarkedMethods { get; private set; }

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (GeneratorUtilities.IsTypeDeclaration(context.Node, CancellationToken.None)

                && MemberHasAttribute(AttributeNames.AutoConstruct, context, cancellationToken)

                && context.SemanticModel.GetDeclaredSymbol(context.Node, cancellationToken)
                    is INamedTypeSymbol type)
            {
                (TypeModels ??= []).Add(TypeModel.Create(type));
            }

            else if (GeneratorUtilities.IsMethodDeclaration(context.Node, CancellationToken.None)

                && MemberHasAttribute(AttributeNames.AutoPostConstruct, context, cancellationToken)

                && context.SemanticModel.GetDeclaredSymbol(context.Node, cancellationToken)
                    is IMethodSymbol method)
            {
                (MarkedMethods ??= []).Add(PostCtorModel.Create(method));
            }
        }

        private static bool MemberHasAttribute(
            string attribute,
            GeneratorSyntaxContext context,
            CancellationToken cancellationToken)
        {
            foreach (var attributeListSyntax in ((MemberDeclarationSyntax)context.Node).AttributeLists)
                foreach (var attributeSyntax in attributeListSyntax.Attributes)
                {
                    if (context.SemanticModel.GetSymbolInfo(attributeSyntax, cancellationToken)
                        .Symbol is not IMethodSymbol attributeSymbol)
                        continue;

                    var attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                    var fullName = attributeContainingTypeSymbol.ToDisplayString();

                    if (fullName != attribute) continue;

                    return true;
                }
            return false;
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
            || receiver.TypeModels == null)
            return;

        var enableGuards = false;
        if (context.AnalyzerConfigOptions.GlobalOptions
            .TryGetValue("build_property.AutoCtorGuards", out var projectGuardSetting))
        {
            enableGuards =
                projectGuardSetting.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                projectGuardSetting.Equals("enable", StringComparison.OrdinalIgnoreCase);
        }

        var models = (
            receiver.TypeModels?.ToImmutableArray() ?? ImmutableArray<TypeModel>.Empty,
            receiver.MarkedMethods?.ToImmutableArray() ?? ImmutableArray<PostCtorModel>.Empty
        );
        Emitter.GenerateSource(context, (models, enableGuards));
    }
}
