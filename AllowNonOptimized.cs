﻿using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Validators;

namespace OrmBenchmark;

public class AllowNonOptimized : ManualConfig
{
    public AllowNonOptimized()
    {
        AddValidator(JitOptimizationsValidator.DontFailOnError); // ALLOW NON-OPTIMIZED DLLs

        AddLogger(DefaultConfig.Instance.GetLoggers().ToArray()); // manual config has no loggers by default
        AddExporter(DefaultConfig.Instance.GetExporters().ToArray()); // manual config has no exporters by default
        AddColumnProvider(DefaultConfig.Instance.GetColumnProviders().ToArray()); // manual config has no columns by default
    }
}