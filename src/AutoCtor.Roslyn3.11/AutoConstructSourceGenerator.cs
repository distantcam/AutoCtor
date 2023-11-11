using System.Collections.Immutable;
using AutoCtor.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoCtor;

[Generator(LanguageNames.CSharp)]
public sealed partial class AutoConstructSourceGenerator : ISourceGenerator
{
    private sealed class SyntaxContextReceiver : ISyntaxContextReceiver
    {
        private readonly CancellationToken _cancellationToken;

        public SyntaxContextReceiver(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        public List<TypeModel>? TypeModels { get; private set; }
        public List<IMethodSymbol>? MarkectMethods { get; private set; }


        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is TypeDeclarationSyntax { AttributeLists.Count: > 0 })
            {
                var type = Parser.GetMarkedNamedTypeSymbol(context, _cancellationToken);
                if (type != null)
                    (TypeModels ??= new()).Add(TypeModel.Create(type));
            }
            else if (context.Node is MethodDeclarationSyntax { AttributeLists.Count: > 0 })
            {
                var method = Parser.GetMarkedMethodSymbol(context, _cancellationToken);
                if (method != null)
                    (MarkectMethods ??= new()).Add(method);
            }
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(static () =>
            new SyntaxContextReceiver(CancellationToken.None));

        context.RegisterForPostInitialization(static c =>
            c.AddSource(AttributeEmitter.HintName, AttributeEmitter.GenerateSource()));
    }

    public void Execute(GeneratorExecutionContext executionContext)
    {
        if (executionContext.SyntaxContextReceiver is not SyntaxContextReceiver receiver
            || receiver.TypeModels == null)
            return;

        var models = (
            receiver.TypeModels?.ToImmutableArray() ?? ImmutableArray<TypeModel>.Empty,
            receiver.MarkectMethods?.ToImmutableArray() ?? ImmutableArray<IMethodSymbol>.Empty
        );
        Emitter.GenerateSource(executionContext, models);
    }
}
