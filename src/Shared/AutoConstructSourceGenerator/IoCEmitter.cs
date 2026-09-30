using Microsoft.CodeAnalysis;
using static AutoCtor.Diagnostics;

#if ROSLYN_3
using EmitterContext = Microsoft.CodeAnalysis.GeneratorExecutionContext;
#elif ROSLYN_4
using EmitterContext = Microsoft.CodeAnalysis.SourceProductionContext;
#endif

namespace AutoCtor;

public partial class AutoConstructSourceGenerator
{
    private static class IoCEmitter
    {
        private const string DI = "global::Microsoft.Extensions.DependencyInjection.";

        private sealed class Node(int id, int order, RegistrationModel registration, ITypeSymbol service, INamedTypeSymbol implementation)
        {
            public string Name { get; } = $"S{id}";
            // Registration order, which decides the winner of a single resolve.
            public int Order { get; } = order;
            public RegistrationModel R { get; } = registration;
            public string Service { get; } = service.ToDisplayString(FullyQualifiedFormat);
            public INamedTypeSymbol Implementation { get; } = implementation;
            public List<Node> Dependencies { get; } = [];
            public string Create { get; set; } = "";
            public bool Scoped { get; set; } = registration.Lifetime == Lifetime.Scoped;
            public int VisitState { get; set; }
        }

        public static void Generate(
            EmitterContext context,
            ServiceProviderModel provider,
            Dictionary<string, ParameterList> ctorMaps,
            DuckTypes duck)
        {
            if (ReportDiagnostics(context, provider.Diagnostics))
                return;

            if (provider.Registrations.Count == 0)
                return;

            List<string> builtIns = ["global::System.IServiceProvider"];
            if (duck.DI)
            {
                builtIns.Add(DI + "IServiceScopeFactory");
                builtIns.Add(DI + "IServiceProviderIsService");
            }
            if (duck.Keyed)
            {
                builtIns.Add(DI + "IKeyedServiceProvider");
                builtIns.Add(DI + "IServiceProviderIsKeyedService");
            }

            var nodes = CreateNodes(context, provider, ctorMaps, duck, builtIns, out var failed);

            foreach (var node in nodes)
                failed |= !VerifyNodes(context, node);

            if (failed)
                return;

            // Registrations first, so they win over collections and built ins of the same type.
            var singles = new List<(string Type, string Key, string Value, bool Scoped)>();
            var collections = new List<(string Type, string Key, string Value, bool Scoped)>();

            foreach (var group in nodes.GroupBy(n => (n.Service, n.R.Key)))
            {
                var ordered = group.OrderBy(n => n.Order).ToList();
                var last = ordered[ordered.Count - 1];
                singles.Add((group.Key.Service, group.Key.Key ?? "null", $"{last.Name}()", last.Scoped));
                collections.Add(($"global::System.Collections.Generic.IEnumerable<{group.Key.Service}>", group.Key.Key ?? "null",
                    NewArray(group.Key.Service, ordered), ordered.Any(n => n.Scoped)));
            }

            if (duck.Keyed)
            {
                // KeyedService.AnyKey asks for every keyed registration of a service.
                foreach (var group in nodes.Where(n => n.R.Key is not null).GroupBy(n => n.Service))
                {
                    var ordered = group.OrderBy(n => n.Order).ToList();
                    collections.Add(($"global::System.Collections.Generic.IEnumerable<{group.Key}>", DI + "KeyedService.AnyKey",
                        NewArray(group.Key, ordered), ordered.Any(n => n.Scoped)));
                }
            }

            var entries = singles.Concat(collections).Concat(builtIns.Select(b => (b, "null", "this", false))).ToList();

            var source = new CodeBuilder()
                .AppendHeader()
                .AppendLine()
                .AppendLine("#nullable enable")
                .AppendLine();

            List<string> baseTypes = [
                .. builtIns,
                "global::System.IDisposable"
            ];
            if (duck.Async)
                baseTypes.Add("global::System.IAsyncDisposable");

            using (source.StartPartialType(provider, [
                .. baseTypes,
                .. Resolvers(entries, false).Select(t => $"{provider.Name}.IResolver<{t}>")]))
            {
                if (duck.DI)
                    baseTypes.Add(DI + "IServiceScope");

                EmitContainer(source, provider, nodes, entries, duck, inScope: false);
                source.AppendLine();
                source.AddGeneratedCodeAttribute();
                using (source.StartType("public sealed class Scope", [
                    .. baseTypes,
                    .. Resolvers(entries, true).Select(t => $"{provider.Name}.IResolver<{t}>")]))
                    EmitContainer(source, provider, nodes, entries, duck, inScope: true);
            }

            context.AddSource($"{provider.HintName}.ServiceProvider.g.cs", source);
        }

        private static bool ReportDiagnostics(EmitterContext context, IEnumerable<ModelDiagnostic> diagnostics)
        {
            var hasError = false;
            foreach (var diagnostic in diagnostics)
            {
                context.ReportDiagnostic(diagnostic, diagnostic.Descriptor, [.. diagnostic.Args]);
                hasError = hasError || diagnostic.Descriptor.DefaultSeverity == DiagnosticSeverity.Error;
            }
            return hasError;
        }

        // The first registration of each unkeyed type wins, the same as in Resolve.
        private static IEnumerable<string> Resolvers(List<(string Type, string Key, string Value, bool Scoped)> entries, bool inScope)
            => entries.Where(e => e.Key == "null" && (inScope || !e.Scoped)).Select(e => e.Type).Distinct();

        // Every registration of a service, closing any open generic rule that matches it.
        // A rule whose constraints the type arguments break is skipped, and named in unsatisfied.
        private static List<Node> Find(
            ServiceProviderModel provider,
            List<Node> nodes,
            ITypeSymbol type,
            string? key,
            out RegistrationModel? unsatisfied)
        {
            var registrations = provider.Registrations;
            unsatisfied = null;
            var name = type.ToDisplayString(FullyQualifiedFormat);
            if (type is INamedTypeSymbol { IsGenericType: true } closed)
            {
                var definition = closed.OriginalDefinition.ToDisplayString(FullyQualifiedFormat);
                for (var i = 0; i < registrations.Count; i++)
                {
                    var r = registrations[i];
                    if (r.IsOpenGeneric && r.Key == key
                        && r.Service.ToString() == definition
                        && r.Implementation.TypeSymbol is INamedTypeSymbol open
                        && open.Arity == closed.Arity
                        && !nodes.Any(n => n.Order == i && n.Service == name))
                    {
                        if (!SatisfiesConstraints(open, closed.TypeArguments))
                        {
                            unsatisfied = r;
                            continue;
                        }
                        nodes.Add(new(nodes.Count, i, r, closed, open.Construct([.. closed.TypeArguments])));
                    }
                }
            }
            return nodes.Where(n => n.Service == name && n.R.Key == key).OrderBy(n => n.Order).ToList();
        }

        private static List<Node> CreateNodes(
            EmitterContext context,
            ServiceProviderModel provider,
            Dictionary<string, ParameterList> ctorMaps,
            DuckTypes duck,
            List<string> builtIns,
            out bool failed)
        {
            failed = false;

            var nodes = new List<Node>();

            for (var i = 0; i < provider.Registrations.Count; i++)
            {
                var r = provider.Registrations[i];
                if (!r.IsOpenGeneric)
                    nodes.Add(new(
                        id: nodes.Count,
                        order: i,
                        registration: r,
                        service: r.Service.TypeSymbol,
                        implementation: (INamedTypeSymbol)r.Implementation.TypeSymbol));
            }

            // Closing an open generic adds a node, so this walks the list as it grows.
            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                IEnumerable<ParameterModel> parameters = node.R.Parameters;
                if (node.R.Factory is null)
                {
                    if (node.R.IsAutoConstruct
                        && ctorMaps.TryGetValue(TypeModel.CreateKey(node.Implementation), out var predicted))
                    {
                        // The constructor AutoCtor is about to generate.
                        parameters = predicted.Distinct();
                    }
                    else if (node.R.PublicConstructorCount != 1)
                    {
                        context.ReportDiagnostic(node.R,
                            ACTR010_ServiceImplementationMustHaveSinglePublicConstructor);
                        failed = true;
                        continue;
                    }
                }

                var typeParameters = node.Implementation.OriginalDefinition.TypeParameters
                    .Select(ConvertToEquatable)
                    .ToEquatableList();
                var typeArguments = node.Implementation.TypeArguments
                    .Select(ConvertToEquatable)
                    .ToEquatableList();
                var argList = parameters
                    .Select(p => Argument(
                        context,
                        provider,
                        duck,
                        builtIns,
                        nodes,
                        node,
                        type: Emitter.SetGenerics(p.Type.TypeSymbol, typeParameters, typeArguments),
                        p.KeyedService,
                        p.ErrorName))
                    .ToList();

                if (argList.Contains(null))
                    failed = true;

                var args = string.Join(", ", argList);

                node.Create = node.R.Factory is { } factory
                    ? node.R.FactoryIsMethod ? $"{factory}({args})" : factory
                    : $"new {node.Implementation.ToDisplayString(FullyQualifiedFormat)}({args})";
            }

            return nodes;
        }

        // The expression that resolves one constructor argument, or null after reporting why it can't.
        private static string? Argument(
            EmitterContext context,
            ServiceProviderModel provider,
            DuckTypes duck,
            List<string> builtIns,
            List<Node> nodes,
            Node node,
            ITypeSymbol type,
            string? key,
            string parameterName)
        {
            var name = type.ToDisplayString(FullyQualifiedFormat);
            var typeName = type.ToDisplayString(MinimallyQualifiedFormat);
            var found = Find(provider, nodes, type, key, out var unsatisfied);
            if (found.Count > 0)
            {
                node.Dependencies.Add(found[found.Count - 1]);
                return $"{found[found.Count - 1].Name}()";
            }
            if (unsatisfied is { } rule)
            {
                context.ReportDiagnostic(rule, ACTR020_OpenGenericConstraintNotSatisfied, typeName, parameterName, rule.ErrorName);
                return null;
            }

            if (type is INamedTypeSymbol { ConstructedFrom.SpecialType: SpecialType.System_Collections_Generic_IEnumerable_T } enumerable)
            {
                var elements = Find(provider, nodes, enumerable.TypeArguments[0], key, out _);
                node.Dependencies.AddRange(elements);
                return NewArray(enumerable.TypeArguments[0].ToDisplayString(FullyQualifiedFormat), elements);
            }

            if (key is null && builtIns.Contains(name))
                return "this";

            if (provider.Fallback is not null && (key is null || duck.Keyed))
                return $"Required<{name}>(GetKeyedService(typeof({name}), {key ?? "null"}))";

            if (key is null)
                context.ReportDiagnostic(node.R, ACTR012_ServiceDependencyNotRegistered, typeName, node.R.ErrorName);
            else
                context.ReportDiagnostic(node.R, ACTR014_KeyedServiceDependencyNotRegistered, typeName, node.R.ErrorName, key);
            return null;
        }

        // Cycles, and which services can only live in a scope. False when an error was reported.
        private static bool VerifyNodes(EmitterContext context, Node node)
        {
            if (node.VisitState == 2)
                return true;
            if (node.VisitState == 1)
            {
                context.ReportDiagnostic(node.R, ACTR013_CircularServiceDependency);
                return false;
            }

            var ok = true;
            node.VisitState = 1;
            foreach (var dependency in node.Dependencies)
            {
                ok &= VerifyNodes(context, dependency);
                node.Scoped |= dependency.Scoped;
                if (node.R.Lifetime != Lifetime.Singleton)
                    continue;

                if (dependency.Scoped)
                {
                    context.ReportDiagnostic(node.R, ACTR019_ScopedServiceCapturedBySingleton, dependency.R.ErrorName, node.R.ErrorName);
                    ok = false;
                }
                else if (dependency.R.Lifetime == Lifetime.Transient)
                {
                    context.ReportDiagnostic(node.R, ACTR018_TransientServiceCaptured, dependency.R.ErrorName, node.R.ErrorName);
                }
            }
            node.VisitState = 2;
            return ok;
        }

        private static bool SatisfiesConstraints(INamedTypeSymbol definition, IReadOnlyList<ITypeSymbol> arguments)
        {
            var typeParameters = definition.TypeParameters
                .Select(ConvertToEquatable)
                .ToEquatableList();
            var typeArguments = arguments
                .Select(ConvertToEquatable)
                .ToEquatableList();

            return definition.TypeParameters.Zip(arguments, (p, a) =>
                (!p.HasReferenceTypeConstraint || a.IsReferenceType)
                && (!p.HasValueTypeConstraint || a.IsValueType)
                && (!p.HasUnmanagedTypeConstraint || a.IsUnmanagedType)
                && (!p.HasConstructorConstraint || a.IsValueType
                    || a is INamedTypeSymbol { IsAbstract: false } named && named.InstanceConstructors.Any(c => c.Parameters.Length == 0 && c.DeclaredAccessibility == Accessibility.Public))
                && p.ConstraintTypes.All(c => ServiceProviderModel.IsAssignable(a, Emitter.SetGenerics(c, typeParameters, typeArguments))))
                .All(ok => ok);
        }

        private static string NewArray(string elementType, List<Node> elements)
            => $"new {elementType}[] {{ {string.Join(", ", elements.Select(static n => $"{n.Name}()"))} }}";

        private static void EmitContainer(
            CodeBuilder source,
            ServiceProviderModel provider,
            List<Node> nodes,
            List<(string Type, string Key, string Value, bool Scoped)> entries,
            DuckTypes duck,
            bool inScope)
        {
            var root = inScope ? "_root" : "this";

            source.AppendLine("private readonly object _lock = new object();");
            source.AppendLine("private global::System.Collections.Generic.List<object>? _disposables;");
            if (inScope)
            {
                source.AppendLine($"private readonly {provider.Name} _root;");
                source.AppendLine($"public Scope({provider.Name} root) => _root = root;");
                source.AppendLine("public global::System.IServiceProvider ServiceProvider => this;");
            }
            source.AppendLine();

            foreach (var node in nodes)
            {
                if (node.Scoped && !inScope)
                    continue;

                var (name, type, create) = (node.Name, node.Service, node.Create.Replace("{root}", root));
                var tracked = node.R.ImplementationIsDisposable ? $"Track<{type}>({create})" : create;
                if (inScope && node.R.Lifetime == Lifetime.Singleton)
                {
                    source.AppendLine($"private {type} {name}() => _root.{name}();");
                }
                else if (node.R.Lifetime == Lifetime.Transient)
                {
                    source.AppendLine($"private {type} {name}() => {tracked};");
                }
                else
                {
                    // Only the first resolve takes the lock. volatile needs a reference type, so a
                    // value type is held boxed.
                    var (field, cast) = node.R.ServiceIsReferenceType
                        ? ($"{type}?", "")
                        : ("object?", $"({type})");
                    source.AppendLine($"private volatile {field} _{name};");
                    using (source.StartBlock($"private {type} {name}()"))
                    {
                        source.AppendLine($"var service = _{name};");
                        source.AppendLine("if (service is not null)")
                            .IncreaseIndent()
                            .AppendLine($"return {cast}service;")
                            .DecreaseIndent();
                        source.AppendLine("lock (_lock)")
                            .IncreaseIndent()
                            .AppendLine($"return {cast}(_{name} ??= {tracked});")
                            .DecreaseIndent();
                    }
                }
            }
            source.AppendLine();

            var visible = entries.Where(e => inScope || !e.Scoped).ToList();
            using (source.StartBlock("private object? Resolve(global::System.Type type, object? key, bool probe)"))
            {
                using (source.StartBlock("if (key is null)"))
                {
                    foreach (var (type, _, value, _) in visible.Where(e => e.Key == "null"))
                        source.AppendLine($"if (type == typeof({type}))")
                            .IncreaseIndent()
                            .AppendLine($"return probe ? this : (object)({value});")
                            .DecreaseIndent();
                    source.AppendLine("return null;");
                }
                foreach (var (type, key, value, _) in visible.Where(e => e.Key != "null"))
                    source.AppendLine($"if (type == typeof({type}) && object.Equals(key, {key}))")
                        .IncreaseIndent()
                        .AppendLine($"return probe ? this : (object)({value});")
                        .DecreaseIndent();
                source.AppendLine("return null;");
            }
            source.AppendLine();

            // A scope gets its own scope of the fallback, so the fallback's scoped services work.
            var fallback = provider.Fallback?.Replace("{root}", root) ?? "null";
            if (inScope && duck.DI && provider.Fallback is not null)
            {
                source.AppendLine("private global::System.IServiceProvider? _fallback;");
                source.AppendLine($"private global::System.IServiceProvider? Fallback {{ get {{ lock (_lock) return _fallback ??= {fallback} is {DI}IServiceScopeFactory f ? Track(f.CreateScope()).ServiceProvider : {fallback}; }} }}");
            }
            else
            {
                source.AppendLine($"private global::System.IServiceProvider? Fallback => {fallback};");
            }

            var keyedFallback = duck.Keyed ? $"(Fallback as {DI}IKeyedServiceProvider)?.GetKeyedService(serviceType, serviceKey)" : "null";
            var fallbackProbe = !duck.DI ? ""
                : $" || (serviceKey == null ? Fallback is {DI}IServiceProviderIsService s && s.IsService(serviceType) : "
                    + (duck.Keyed ? $"Fallback is {DI}IServiceProviderIsKeyedService k && k.IsKeyedService(serviceType, serviceKey))" : "false)");

            source.AppendLine()
                .AppendLine("public object? GetService(global::System.Type serviceType)")
                .AppendLine("\t=> GetKeyedService(serviceType, null);");

            source.AppendLine()
                .AppendLine("public object? GetKeyedService(global::System.Type serviceType, object? serviceKey)")
                .AppendLine("\t=> Resolve(serviceType, serviceKey, false)")
                .AppendLine($"\t?? (serviceKey == null ? Fallback?.GetService(serviceType) : {keyedFallback});");

            source.AppendLine()
                .AppendLine("public object GetRequiredKeyedService(global::System.Type serviceType, object? serviceKey)")
                .AppendLine("\t=> GetKeyedService(serviceType, serviceKey)")
                .AppendLine("\t?? throw new global::System.InvalidOperationException(\"No service for type '\" + serviceType + \"' has been registered.\");");

            // Typed lookups: an interface check instead of a chain of type comparisons.
            source.AppendLine()
                .AppendLine("public T? GetService<T>()")
                .AppendLine($"\t=> this is {provider.Name}.IResolver<T> resolver ? resolver.Get() : GetService(typeof(T)) is T service ? service : default;");

            source.AppendLine()
                .AppendLine("public T GetRequiredService<T>()")
                .AppendLine($"\t=> this is {provider.Name}.IResolver<T> resolver ? resolver.Get() : (T)GetRequiredKeyedService(typeof(T), null);");

            source.AppendLine();
            if (!inScope)
                source.AppendLine("private interface IResolver<T> { T Get(); }");
            foreach (var type in Resolvers(entries, inScope))
                source
                    .AppendLine($"{type} {provider.Name}.IResolver<{type}>.Get()")
                    .AppendLine($"\t=> {visible.First(e => e.Type == type && e.Key == "null").Value};");

            source.AppendLine()
                .AppendLine("public bool IsService(global::System.Type serviceType)")
                .AppendLine("\t=> IsKeyedService(serviceType, null);");

            source.AppendLine()
                .AppendLine($"public bool IsKeyedService(global::System.Type serviceType, object? serviceKey)")
                .AppendLine($"\t=> Resolve(serviceType, serviceKey, true) != null{fallbackProbe};");

            source.AppendLine();
            source.AppendLine($"public Scope CreateScope() => new Scope({root});");
            if (duck.DI)
                source.AppendLine($"{DI}IServiceScope {DI}IServiceScopeFactory.CreateScope() => CreateScope();");

            source.AppendLine()
                .AppendLine("private static T Required<T>(object? service)")
                .AppendLine("\t=> service is T t ? t : throw new global::System.InvalidOperationException(\"No service for type '\" + typeof(T) + \"' has been registered.\");");

            source.AppendLine();
            using (source.StartBlock("private T Track<T>(T service)"))
            {
                source.AppendLine($"if (service is global::System.IDisposable{(duck.Async ? " || service is global::System.IAsyncDisposable" : "")})");
                source.AppendLine("\tlock (_lock)");
                source.AppendLine("\t\t(_disposables ??= new global::System.Collections.Generic.List<object>()).Add(service);");
                source.AppendLine("return service;");
            }

            // Disposed in reverse order of creation, and only once.
            source.AppendLine();
            using (source.StartBlock("private object[] Drain()"))
            using (source.StartBlock("lock (_lock)"))
            {
                source.AppendLine("var items = _disposables?.ToArray() ?? new object[0];");
                source.AppendLine("_disposables = null;");
                source.AppendLine("global::System.Array.Reverse(items);");
                source.AppendLine("return items;");
            }

            source.AppendLine();
            using (source.StartBlock("public void Dispose()"))
            using (source.StartBlock("foreach (var item in Drain())"))
            using (source.StartBlock("if (item is global::System.IDisposable disposable)"))
            {
                source.AppendLine("disposable.Dispose();");
            }

            if (duck.Async)
            {
                source.AppendLine();
                using (source.StartBlock("public async global::System.Threading.Tasks.ValueTask DisposeAsync()"))
                using (source.StartBlock("foreach (var item in Drain())"))
                {
                    source.AppendLine("if (item is global::System.IAsyncDisposable d)");
                    source.AppendLine("\tawait d.DisposeAsync().ConfigureAwait(false);");
                    source.AppendLine("else");
                    source.AppendLine("\t((global::System.IDisposable)item).Dispose();");
                }
            }
        }

        private static EquatableTypeSymbol ConvertToEquatable(ITypeSymbol typeSymbol) => new(typeSymbol);
    }
}
