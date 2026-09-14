﻿using System.Reflection;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;

// Microsoft.CodeAnalysis.Analyzer.Testing ships non-optimized. It is only used to resolve
// reference assemblies during setup, never on a measured path, so the validator is disabled.
var config = ManualConfig.Create(DefaultConfig.Instance)
    .WithOptions(ConfigOptions.DisableLogFile | ConfigOptions.DisableOptimizationsValidator)
    .WithOrderer(new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest));

BenchmarkRunner.Run(Assembly.GetExecutingAssembly(), config);
