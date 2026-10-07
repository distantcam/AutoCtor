using System.Reflection;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;

var config = ManualConfig.Create(DefaultConfig.Instance)
    .WithOptions(ConfigOptions.DisableLogFile)
    .WithOrderer(new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest));

BenchmarkRunner.Run(Assembly.GetExecutingAssembly(), config);
