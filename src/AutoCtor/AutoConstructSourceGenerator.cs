using System.Collections.Immutable;
using AutoSource;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace AutoCtor;

[Generator(LanguageNames.CSharp)]
public class AutoConstructSourceGenerator : IIncrementalGenerator
{
    public static readonly DiagnosticDescriptor AmbiguousMarkedPostConstructMethodWarning = new DiagnosticDescriptor(
        id: "ACTR001",
        title: "Ambiguous marked post constructor method",
        messageFormat: "Only one method in a type should be marked with an [AutoPostConstruct] attribute",
        category: "AutoCtor",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor PostConstructMethodNotVoidWarning = new DiagnosticDescriptor(
        id: "ACTR002",
        title: "Post construct method must return void",
        messageFormat: "The method '{0}' must return void to be used as the post construct method",
        category: "AutoCtor",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor PostConstructMethodHasOptionalArgsWarning = new DiagnosticDescriptor(
        id: "ACTR003",
        title: "Post construct method must not have any optional arguments",
        messageFormat: "The method '{0}' must not have optional arguments to be used as the post construct method",
        category: "AutoCtor",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor PostConstructMethodCannotBeGenericWarning = new DiagnosticDescriptor(
        id: "ACTR004",
        title: "Post construct method must not be generic",
        messageFormat: "The method '{0}' must not be generic to be used as the post construct method",
        category: "AutoCtor",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var types = context.SyntaxProvider.CreateSyntaxProvider(
            static (s, ct) => SourceTools.IsCorrectAttribute("AutoConstruct", s, ct),
            SourceTools.GetTypeFromAttribute)
            .Where(x => x != null)
            .Collect();

        context.RegisterSourceOutput(types, GenerateSource);
    }

    private static void GenerateSource(SourceProductionContext context, ImmutableArray<ITypeSymbol?> types)
    {
        if (types.IsDefaultOrEmpty) return;

        var ctorMaps = new Dictionary<ITypeSymbol, ParameterList>(SymbolEqualityComparer.Default);
        var orderedTypes = types.OfType<INamedTypeSymbol>().OrderBy(static t =>
        {
            var count = 0;
            var b = t.BaseType;
            while (b != null)
            {
                count++;
                b = b.BaseType;
            }
            return count;
        });

        foreach (var type in orderedTypes)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            IEnumerable<Parameter>? baseParameters = default;

            if (type.BaseType != null)
            {
                if (type.BaseType.IsGenericType)
                {
                    if (ctorMaps.TryGetValue(type.BaseType.ConstructUnboundGenericType(), out var temp))
                    {
                        var baseParameterList = new List<Parameter>();
                        var typedArgs = type.BaseType.TypeArguments;
                        var typedParameters = type.BaseType.TypeParameters;
                        foreach (var bp in temp)
                        {
                            var bpName = bp.Name;
                            var bpType = bp.Type;
                            for (var i = 0; i < typedParameters.Length; i++)
                            {
                                if (SymbolEqualityComparer.Default.Equals(typedParameters[i], bp.Type))
                                {
                                    bpType = typedArgs[i];
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
                    ctorMaps.TryGetValue(type.BaseType, out var temp);
                    baseParameters = temp?.ToList();
                }
            }

            (var source, var parameters) = GenerateSource(context, type, baseParameters);

            if (type.IsGenericType)
                ctorMaps.Add(type.ConstructUnboundGenericType(), parameters);
            else
                ctorMaps.Add(type, parameters);

            var hintName = type.ToDisplayString(GeneratorUtilities.HintSymbolDisplayFormat)
                .Replace('<', '[')
                .Replace('>', ']');

            context.AddSource($"{hintName}.g.cs", source);
        }
    }

    private static (SourceText, ParameterList) GenerateSource(
        SourceProductionContext context,
        ITypeSymbol type,
        IEnumerable<Parameter>? baseParameters = default)
    {
        var ns = type.ContainingNamespace.IsGlobalNamespace
                ? null
                : type.ContainingNamespace.ToString();

        var fields = type.GetMembers().OfType<IFieldSymbol>()
            .Where(f => f.IsReadOnly && !f.IsStatic && f.CanBeReferencedByName && !HasFieldInitialiser(f));

        var autoCtorAttribute = type.GetAttributes().First(a => a.AttributeClass?.Name == nameof(AutoConstructAttribute));
        var loc = autoCtorAttribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken)?.GetLocation();

        var postCtorMethod = GetPostCtorMethod(context, type, loc);

        var parameters = new ParameterList(fields);
        if (type.BaseType != null)
        {
            var constructor = type.BaseType.Constructors.OnlyOrDefault(c => !c.IsStatic && c.Parameters.Any());
            if (constructor != null)
            {
                parameters.AddParameters(constructor.Parameters);
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

        using (source.StartPartialType(type))
        {
            source.AddCompilerGeneratedAttribute().AddGeneratedCodeAttribute();

            if (parameters.HasBaseParameters)
            {
                source.AppendLine($"public {type.Name}({parameters.ToParameterString()}) : base({parameters.ToBaseParameterString()})");
            }
            else
            {
                source.AppendLine($"public {type.Name}({parameters.ToParameterString()})");
            }

            source.StartBlock();
            foreach (var f in fields)
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

    private static IMethodSymbol? GetPostCtorMethod(
        SourceProductionContext context,
        ITypeSymbol type,
        Location? attributeLocation)
    {
        var markedPostCtorMethods = type.GetMembers().OfType<IMethodSymbol>()
            .Where(m => m.GetAttributes().Any(a => a.AttributeClass?.Name == nameof(AutoPostConstructAttribute)));

        // ACTR002
        if (markedPostCtorMethods.MoreThan(1))
        {
            foreach (var loc in markedPostCtorMethods.SelectMany(static m => m.Locations))
                context.ReportDiagnostic(Diagnostic.Create(AmbiguousMarkedPostConstructMethodWarning, loc));
            return null;
        }

        if (!markedPostCtorMethods.Any())
            return null;

        var method = markedPostCtorMethods.First();

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

