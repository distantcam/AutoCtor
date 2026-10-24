using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

namespace CompileBenchmarks;

internal class DefaultConfig : ManualConfig
{
    private const string NugetVersion = "4.0.1";

    public DefaultConfig()
    {
        AddJob(Job.ShortRun
            .WithMsBuildArguments("/p:AutoCtorVersion=local")
            .WithId("local"));
        AddJob(Job.ShortRun
            .WithMsBuildArguments($"/p:AutoCtorVersion={NugetVersion}")
            .WithId(NugetVersion)
            .AsBaseline());

        HideColumns(Column.Arguments);
        AddLogicalGroupRules(BenchmarkLogicalGroupRule.ByParams);
    }
}
