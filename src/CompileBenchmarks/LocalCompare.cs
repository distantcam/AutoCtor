﻿using System.Text;
using AutoCtor;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

namespace CompileBenchmarks;

[Config(typeof(Config))]
public class LocalCompare
{
    [Params(100, 1000)]
    public int ServiceCount { get; set; }

    [Params(100, 1000)]
    public int TargetsCount { get; set; }

    private class Config : ManualConfig
    {
        public Config()
        {
            AddJob(Job.ShortRun
                .WithMsBuildArguments("/p:AutoCtorVersion=local")
                .WithId("local"));
            AddJob(Job.ShortRun
                .WithMsBuildArguments("/p:AutoCtorVersion=3.1.1")
                .WithId("3.1.1")
                .AsBaseline());

            HideColumns(Column.Arguments);
            AddLogicalGroupRules(BenchmarkLogicalGroupRule.ByParams);
        }
    }

    private CSharpCompilation _compilation = null!;
    private GeneratorDriver _driver = null!;

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        var frameworkReferences = await ReferenceAssemblies.Net.Net100
            .ResolveAsync(CSharpParseOptions.Default.Language, CancellationToken.None)
            .ConfigureAwait(false);

        _compilation = CSharpCompilation.Create(nameof(LocalCompare))
            .WithOptions(new(OutputKind.DynamicallyLinkedLibrary))
            .AddReferences(frameworkReferences)
            .AddReferences(MetadataReference
                .CreateFromFile(typeof(AutoConstructAttribute).Assembly.Location))
            .AddSyntaxTrees(Enumerable.Range(1, ServiceCount)
                .Select(i => CSharpSyntaxTree.ParseText($"public interface IService{i:D4};")))
            .AddSyntaxTrees(Enumerable.Range(1, TargetsCount).Select(TargetClassGenerator));

        _driver = CSharpGeneratorDriver.Create(new AutoConstructSourceGenerator());
    }

    private SyntaxTree TargetClassGenerator(int i)
    {
        var rng = new Random(i);

        var sb = new StringBuilder();

        sb.AppendLine("[AutoCtor.AutoConstruct]");
        sb.AppendLine($"public class Target{i:D4}");
        sb.AppendLine("{");

        var count = rng.Next(ServiceCount);
        var items = rng.GetItems(Enumerable.Range(1, ServiceCount).ToArray(), count);
        foreach (var item in items)
        {
            sb.Append($"    private readonly IService{item:D4} _service{item:D4};");
        }

        sb.AppendLine("}");

        return CSharpSyntaxTree.ParseText(sb.ToString());
    }

    [Benchmark]
    public GeneratorDriver Standard() => _driver.RunGenerators(_compilation);
}
