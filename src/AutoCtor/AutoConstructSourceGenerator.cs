using System.Collections.Immutable;
using AutoSource;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace AutoCtor;

[Generator(LanguageNames.CSharp)]
public class AutoConstructSourceGenerator : IIncrementalGenerator
{
    public static readonly DiagnosticDescriptor AmbiguousMarkedPostConstructMethodWarning = new(
        id: "ACTR001",
        title: "Ambiguous marked post constructor method",
        messageFormat: "Only one method in a type should be marked with an [AutoPostConstruct] attribute",
        category: "AutoCtor",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor PostConstructMethodNotVoidWarning = new(
        id: "ACTR002",
        title: "Post construct method must return void",
        messageFormat: "The method '{0}' must return void to be used as the post construct method",
        category: "AutoCtor",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor PostConstructMethodHasOptionalArgsWarning = new(
        id: "ACTR003",
        title: "Post construct method must not have any optional arguments",
        messageFormat: "The method '{0}' must not have optional arguments to be used as the post construct method",
        category: "AutoCtor",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor PostConstructMethodCannotBeGenericWarning = new(
        id: "ACTR004",
        title: "Post construct method must not be generic",
        messageFormat: "The method '{0}' must not be generic to be used as the post construct method",
        category: "AutoCtor",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static bool IsTypeDeclaration(SyntaxNode node, CancellationToken cancellationToken)
        => node is ClassDeclarationSyntax
        || node is RecordDeclarationSyntax
        || node is StructDeclarationSyntax;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var types = context.SyntaxProvider.ForAttributeWithMetadataName(
            "AutoCtor.AutoConstructAttribute",
            IsTypeDeclaration,
            static (c, ct) => new TypeModel((INamedTypeSymbol)c.TargetSymbol, ct))
        .Collect();

        context.RegisterSourceOutput(types, GenerateSource);
    }

    private static void GenerateSource(SourceProductionContext context, ImmutableArray<TypeModel> types)
    {
        if (types.IsDefaultOrEmpty) return;

        var ctorMaps = new Dictionary<string, ParameterList>();
        var orderedTypes = types.OrderBy(static t => t.Data.Depth);

        foreach (var type in orderedTypes)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            IEnumerable<Parameter>? baseParameters = default;

            if (type.Data.HasBaseType)
            {
                if (type.BaseTypeArguments != null && type.BaseTypeParameters != null)
                {
                    if (ctorMaps.TryGetValue(type.Data.BaseTypeKey, out var temp))
                    {
                        var baseParameterList = new List<Parameter>();
                        foreach (var bp in temp)
                        {
                            var bpName = bp.Name;
                            var bpType = bp.Type;
                            for (var i = 0; i < type.BaseTypeParameters.Count; i++)
                            {
                                if (SymbolEqualityComparer.Default.Equals(type.BaseTypeParameters[i], bp.Type))
                                {
                                    bpType = type.BaseTypeArguments[i];
                                    break;
                                }
                            }
                            baseParameterList.Add(new Parameter(bpType, bpName));
                        }
                        baseParameters = baseParameterList;
                    }
                }
                else
                {
                    ctorMaps.TryGetValue(type.Data.BaseTypeKey, out var temp);
                    baseParameters = temp?.ToList();
                }
            }

            (var source, var parameters) = GenerateSource(context, type, baseParameters);

            ctorMaps.Add(type.Data.TypeKey, parameters);

            context.AddSource($"{type.Data.HintName}.g.cs", source);
        }
    }

    private static (SourceText, ParameterList) GenerateSource(
        SourceProductionContext context,
        TypeModel type,
        IEnumerable<Parameter>? baseParameters = default)
    {
        var postCtorMethod = GetPostCtorMethod(context, type);

        var parameters = new ParameterList(type.Fields);
        if (type.Data.HasBaseType)
        {
            if (type.BaseCtorParameters != null)
            {
                parameters.AddParameters(type.BaseCtorParameters);
            }
            else if (baseParameters != null)
            {
                parameters.AddBaseParameters(baseParameters);
            }
        }
        if (postCtorMethod != null)
        {
            parameters.AddPostCtorParameters(postCtorMethod.Parameters);
        }
        parameters.MakeUniqueNames();

        var source = new CodeBuilder()
            .AppendHeader()
            .AppendLine();

        using (source.StartPartialType(type.Data.Namespace, type.TypeDeclarations))
        {
            source.AddCompilerGeneratedAttribute().AddGeneratedCodeAttribute();

            if (parameters.HasBaseParameters)
            {
                source.AppendLine($"public {type.Data.Name}({parameters.ToParameterString()}) : base({parameters.ToBaseParameterString()})");
            }
            else
            {
                source.AppendLine($"public {type.Data.Name}({parameters.ToParameterString()})");
            }

            source.StartBlock();
            foreach (var f in type.Fields)
            {
                source.AppendLine($"this.{f.Name} = {parameters.FieldParameterName(f)};");
            }
            if (postCtorMethod != null)
            {
                source.AppendLine($"{postCtorMethod.Name}({parameters.ToPostCtorParameterString()});");
            }
            source.EndBlock();
        }

        return (source, parameters);
    }

    private static IMethodSymbol? GetPostCtorMethod(SourceProductionContext context, TypeModel type)
    {
        // ACTR002
        if (type.MarkedPostCtorMethods.MoreThan(1))
        {
            foreach (var loc in type.MarkedPostCtorMethods.SelectMany(static m => m.Locations))
                context.ReportDiagnostic(Diagnostic.Create(AmbiguousMarkedPostConstructMethodWarning, loc));
            return null;
        }

        if (!type.MarkedPostCtorMethods.Any())
            return null;

        var method = type.MarkedPostCtorMethods.First();

        // ACTR004
        if (!method.ReturnsVoid)
        {
            foreach (var loc in method.Locations)
                context.ReportDiagnostic(Diagnostic.Create(PostConstructMethodNotVoidWarning, loc, method.Name));
            return null;
        }

        // ACTR005
        if (method.Parameters.Any(static p => p.IsOptional))
        {
            foreach (var loc in method.Locations)
                context.ReportDiagnostic(Diagnostic.Create(PostConstructMethodHasOptionalArgsWarning, loc, method.Name));
            return null;
        }

        // ACTR006
        if (method.IsGenericMethod)
        {
            foreach (var loc in method.Locations)
                context.ReportDiagnostic(Diagnostic.Create(PostConstructMethodCannotBeGenericWarning, loc, method.Name));
            return null;
        }

        return method;
    }

    private static bool HasFieldInitialiser(IFieldSymbol symbol)
    {
        return symbol.DeclaringSyntaxReferences.Select(x => x.GetSyntax()).OfType<VariableDeclaratorSyntax>().Any(x => x.Initializer != null);
    }
}

