﻿using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

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
            INamedTypeSymbol? type;
            IMethodSymbol? method;
            if (GeneratorUtilities.IsTypeDeclarationWithAttributes(context.Node, cancellationToken)

                && (type = GeneratorUtilities.GetSymbol<INamedTypeSymbol>(context, cancellationToken)) != null

                && GeneratorUtilities.HasAttribute(type, AttributeNames.AutoConstruct))
            {
                (TypeModels ??= []).Add(TypeModel.Create(type));
            }

            else if (GeneratorUtilities.IsMethodDeclarationWithAttributes(context.Node, cancellationToken)

                && (method = GeneratorUtilities.GetSymbol<IMethodSymbol>(context, cancellationToken)) != null

                && GeneratorUtilities.HasAttribute(method, AttributeNames.AutoPostConstruct))
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
