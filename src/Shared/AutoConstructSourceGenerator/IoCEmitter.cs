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
            foreach (var diagnostic in provider.Diagnostics)
                context.ReportDiagnostic(diagnostic, diagnostic.Descriptor, [.. diagnostic.Args]);
            if (provider.Diagnostics.Any(d => d.Descriptor.DefaultSeverity == DiagnosticSeverity.Error))
                return;

            var registrations = provider.Registrations;
            var nodes = new List<Node>();
            for (var i = 0; i < registrations.Count; i++)
            {
                var r = registrations[i];
                if (!r.IsOpenGeneric)
                    nodes.Add(new(nodes.Count, i, r, r.Service.TypeSymbol, (INamedTypeSymbol)r.Implementation.TypeSymbol));
            }
            if (registrations.Count == 0)
                return;

            List<string> builtIns = ["global::System.IServiceProvider"];
            if (duck.DI)
                builtIns.AddRange([DI + "IServiceScopeFactory", DI + "IServiceProviderIsService"]);
            if (duck.Keyed)
                builtIns.AddRange([DI + "IKeyedServiceProvider", DI + "IServiceProviderIsKeyedService"]);

            // Every registration of a service, closing any open generic rule that matches it.
            // A rule whose constraints the type arguments break is skipped, and named in unsatisfied.
            List<Node> Find(ITypeSymbol type, string? key, out RegistrationModel? unsatisfied)
            {
                unsatisfied = null;
                var name = type.ToDisplayString(FullyQualifiedFormat);
                if (type is INamedTypeSymbol { IsGenericType: true } closed)
                {
                    var definition = closed.OriginalDefinition.ToDisplayString(FullyQualifiedFormat);
                    for (var i = 0; i < registrations.Count; i++)
                    {
                        var r = registrations[i];
                        if (r.IsOpenGeneric && r.Key == key && r.Service.ToString() == definition
                            && r.Implementation.TypeSymbol is INamedTypeSymbol open && open.Arity == closed.Arity
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

            var failed = false;

            string Argument(Node node, ITypeSymbol type, string? key, string parameterName)
            {
                var name = type.ToDisplayString(FullyQualifiedFormat);
                var typeName = type.ToDisplayString(MinimallyQualifiedFormat);
                var found = Find(type, key, out var unsatisfied);
                if (found.Count > 0)
                {
                    node.Dependencies.Add(found[found.Count - 1]);
                    return $"{found[found.Count - 1].Name}()";
                }
                if (unsatisfied is { } rule)
                {
                    context.ReportDiagnostic(rule, ACTR020_OpenGenericConstraintNotSatisfied, typeName, parameterName, rule.ErrorName);
                    failed = true;
                    return "";
                }

                if (type is INamedTypeSymbol { ConstructedFrom.SpecialType: SpecialType.System_Collections_Generic_IEnumerable_T } enumerable)
                {
                    var elements = Find(enumerable.TypeArguments[0], key, out _);
                    node.Dependencies.AddRange(elements);
                    return Array(enumerable.TypeArguments[0].ToDisplayString(FullyQualifiedFormat), elements);
                }

                if (key is null && builtIns.Contains(name))
                    return "this";

                if (provider.Fallback is not null && (key is null || duck.Keyed))
                    return $"Required<{name}>(GetKeyedService(typeof({name}), {key ?? "null"}))";

                if (key is null)
                    context.ReportDiagnostic(node.R, ACTR012_ServiceDependencyNotRegistered, typeName, node.R.ErrorName);
                else
                    context.ReportDiagnostic(node.R, ACTR014_KeyedServiceDependencyNotRegistered, typeName, node.R.ErrorName, key);
                failed = true;
                return "";
            }

            // Closing an open generic adds a node, so this walks the list as it grows.
            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                IEnumerable<ParameterModel> parameters = node.R.Parameters;
                if (node.R.Factory is null)
                {
                    if (node.R.IsAutoConstruct && ctorMaps.TryGetValue(TypeModel.CreateKey(node.Implementation), out var predicted))
                    {
                        // The constructor AutoCtor is about to generate.
                        parameters = predicted.Distinct();
                    }
                    else if (node.R.PublicConstructorCount != 1)
                    {
                        context.ReportDiagnostic(node.R, ACTR010_ServiceImplementationMustHaveSinglePublicConstructor);
                        failed = true;
                        continue;
                    }
                }

                EquatableList<EquatableTypeSymbol> typeParameters = new(node.Implementation.OriginalDefinition.TypeParameters.Select(t => new EquatableTypeSymbol(t)));
                EquatableList<EquatableTypeSymbol> typeArguments = new(node.Implementation.TypeArguments.Select(t => new EquatableTypeSymbol(t)));
                var args = string.Join(", ", parameters.Select(p =>
                    Argument(node, Emitter.SetGenerics(p.Type.TypeSymbol, typeParameters, typeArguments), p.KeyedService, p.ErrorName)));

                node.Create = node.R.Factory is { } factory
                    ? node.R.FactoryIsMethod ? $"{factory}({args})" : factory
                    : $"new {node.Implementation.ToDisplayString(FullyQualifiedFormat)}({args})";
            }

            // Cycles, and which services can only live in a scope.
            void Visit(Node node)
            {
                if (node.VisitState == 2)
                    return;
                if (node.VisitState == 1)
                {
                    context.ReportDiagnostic(node.R, ACTR013_CircularServiceDependency);
                    failed = true;
                    return;
                }

                node.VisitState = 1;
                foreach (var dependency in node.Dependencies)
                {
                    Visit(dependency);
                    node.Scoped |= dependency.Scoped;
                    if (node.R.Lifetime != Lifetime.Singleton)
                        continue;

                    if (dependency.Scoped)
                    {
                        context.ReportDiagnostic(node.R, ACTR019_ScopedServiceCapturedBySingleton, dependency.R.ErrorName, node.R.ErrorName);
                        failed = true;
                    }
                    else if (dependency.R.Lifetime == Lifetime.Transient)
                    {
                        context.ReportDiagnostic(node.R, ACTR018_TransientServiceCaptured, dependency.R.ErrorName, node.R.ErrorName);
                    }
                }
                node.VisitState = 2;
            }

            foreach (var node in nodes)
                Visit(node);

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
                    Array(group.Key.Service, ordered), ordered.Any(n => n.Scoped)));
            }
            if (duck.Keyed)
            {
                // KeyedService.AnyKey asks for every keyed registration of a service.
                foreach (var group in nodes.Where(n => n.R.Key is not null).GroupBy(n => n.Service))
                {
                    var ordered = group.OrderBy(n => n.Order).ToList();
                    collections.Add(($"global::System.Collections.Generic.IEnumerable<{group.Key}>", DI + "KeyedService.AnyKey",
                        Array(group.Key, ordered), ordered.Any(n => n.Scoped)));
                }
            }
            var entries = singles.Concat(collections).Concat(builtIns.Select(b => (b, "null", "this", false))).ToList();

            var interfaces = string.Join(", ", builtIns) + ", global::System.IDisposable" + (duck.Async ? ", global::System.IAsyncDisposable" : "");

            var source = new CodeBuilder()
                .AppendHeader()
                .AppendLine()
                .AppendLine("#nullable enable")
                .AppendLine();

            string Resolved(bool inScope) => string.Concat(Resolvers(entries, inScope).Select(t => $", {provider.Name}.IResolver<{t}>"));

            using (source.StartPartialType(provider, interfaces + Resolved(false)))
            {
                EmitContainer(source, provider, nodes, entries, duck, inScope: false);
                source.AppendLine();
                source.AddGeneratedCodeAttribute();
                source.AppendLine($"public sealed class Scope : {interfaces}{(duck.DI ? $", {DI}IServiceScope" : "")}{Resolved(true)}");
                using (source.StartBlock())
                    EmitContainer(source, provider, nodes, entries, duck, inScope: true);
            }

            context.AddSource($"{provider.HintName}.ServiceProvider.g.cs", source);
        }

        // The first registration of each unkeyed type wins, the same as in Resolve.
        private static IEnumerable<string> Resolvers(List<(string Type, string Key, string Value, bool Scoped)> entries, bool inScope)
            => entries.Where(e => e.Key == "null" && (inScope || !e.Scoped)).Select(e => e.Type).Distinct();

        private static bool SatisfiesConstraints(INamedTypeSymbol definition, IReadOnlyList<ITypeSymbol> arguments)
        {
            EquatableList<EquatableTypeSymbol> typeParameters = new(definition.TypeParameters.Select(t => new EquatableTypeSymbol(t)));
            EquatableList<EquatableTypeSymbol> typeArguments = new(arguments.Select(t => new EquatableTypeSymbol(t)));
            return definition.TypeParameters.Zip(arguments, (p, a) =>
                (!p.HasReferenceTypeConstraint || a.IsReferenceType)
                && (!p.HasValueTypeConstraint || a.IsValueType)
                && (!p.HasUnmanagedTypeConstraint || a.IsUnmanagedType)
                && (!p.HasConstructorConstraint || a.IsValueType
                    || a is INamedTypeSymbol { IsAbstract: false } named && named.InstanceConstructors.Any(c => c.Parameters.Length == 0 && c.DeclaredAccessibility == Accessibility.Public))
                && p.ConstraintTypes.All(c => ServiceProviderModel.IsAssignable(a, Emitter.SetGenerics(c, typeParameters, typeArguments))))
                .All(ok => ok);
        }

        private static string Array(string elementType, List<Node> elements)
            => $"new {elementType}[] {{ {string.Join(", ", elements.Select(n => $"{n.Name}()"))} }}";

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
                    var (field, cast) = node.R.ServiceIsReferenceType ? ($"{type}?", "") : ("object?", $"({type})");
                    source.AppendLine($"private volatile {field} _{name};");
                    source.AppendLine($"private {type} {name}() {{ var service = _{name}; if (service is not null) return {cast}service; lock (_lock) return {cast}(_{name} ??= {tracked}); }}");
                }
            }
            source.AppendLine();

            var visible = entries.Where(e => inScope || !e.Scoped).ToList();
            using (source.StartBlock("private object? Resolve(global::System.Type type, object? key, bool probe)"))
            {
                using (source.StartBlock("if (key is null)"))
                {
                    foreach (var (type, _, value, _) in visible.Where(e => e.Key == "null"))
                        source.AppendLine($"if (type == typeof({type})) return probe ? this : (object)({value});");
                    source.AppendLine("return null;");
                }
                foreach (var (type, key, value, _) in visible.Where(e => e.Key != "null"))
                    source.AppendLine($"if (type == typeof({type}) && object.Equals(key, {key})) return probe ? this : (object)({value});");
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

            source.AppendLine("public object? GetService(global::System.Type serviceType) => GetKeyedService(serviceType, null);");
            source.AppendLine($"public object? GetKeyedService(global::System.Type serviceType, object? serviceKey) => Resolve(serviceType, serviceKey, false) ?? (serviceKey == null ? Fallback?.GetService(serviceType) : {keyedFallback});");
            source.AppendLine("public object GetRequiredKeyedService(global::System.Type serviceType, object? serviceKey) => GetKeyedService(serviceType, serviceKey) ?? throw new global::System.InvalidOperationException(\"No service for type '\" + serviceType + \"' has been registered.\");");
            // Typed lookups: an interface check instead of a chain of type comparisons.
            source.AppendLine($"public T? GetService<T>() => this is {provider.Name}.IResolver<T> resolver ? resolver.Get() : GetService(typeof(T)) is T service ? service : default;");
            source.AppendLine($"public T GetRequiredService<T>() => this is {provider.Name}.IResolver<T> resolver ? resolver.Get() : (T)GetRequiredKeyedService(typeof(T), null);");
            foreach (var type in Resolvers(entries, inScope))
                source.AppendLine($"{type} {provider.Name}.IResolver<{type}>.Get() => {visible.First(e => e.Type == type && e.Key == "null").Value};");
            if (!inScope)
                source.AppendLine("private interface IResolver<T> { T Get(); }");
            source.AppendLine("public bool IsService(global::System.Type serviceType) => IsKeyedService(serviceType, null);");
            source.AppendLine($"public bool IsKeyedService(global::System.Type serviceType, object? serviceKey) => Resolve(serviceType, serviceKey, true) != null{fallbackProbe};");
            source.AppendLine($"public Scope CreateScope() => new Scope({root});");
            if (duck.DI)
                source.AppendLine($"{DI}IServiceScope {DI}IServiceScopeFactory.CreateScope() => CreateScope();");
            source.AppendLine();

            source.AppendLine("private static T Required<T>(object? service) => service is T t ? t : throw new global::System.InvalidOperationException(\"No service for type '\" + typeof(T) + \"' has been registered.\");");
            using (source.StartBlock("private T Track<T>(T service)"))
            {
                source.AppendLine($"if (service is global::System.IDisposable{(duck.Async ? " || service is global::System.IAsyncDisposable" : "")}) lock (_lock) (_disposables ??= new global::System.Collections.Generic.List<object>()).Add(service);");
                source.AppendLine("return service;");
            }
            // Disposed in reverse order of creation, and only once.
            source.AppendLine("private object[] Drain() { lock (_lock) { var items = _disposables?.ToArray() ?? new object[0]; _disposables = null; global::System.Array.Reverse(items); return items; } }");
            source.AppendLine("public void Dispose() { foreach (var item in Drain()) (item as global::System.IDisposable ?? throw new global::System.InvalidOperationException(\"'\" + item.GetType() + \"' only implements IAsyncDisposable, use DisposeAsync.\")).Dispose(); }");
            if (duck.Async)
            {
                using (source.StartBlock("public async global::System.Threading.Tasks.ValueTask DisposeAsync()"))
                {
                    source.AppendLine("foreach (var item in Drain())");
                    source.AppendLine("\tif (item is global::System.IAsyncDisposable d) await d.DisposeAsync().ConfigureAwait(false);");
                    source.AppendLine("\telse ((global::System.IDisposable)item).Dispose();");
                }
            }
        }
    }
}
