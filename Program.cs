using System.Configuration;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using OrmBenchmark;
using Perfolizer.Horology;

Console.WriteLine("Seeding data");
await Generator.SeedData(Convert.ToBoolean(ConfigurationManager.AppSettings["CleanDatabase"]));
#if DEBUG
BenchmarkRunner.Run<RunBenchmark>(new AllowNonOptimized
    { SummaryStyle = SummaryStyle.Default.WithTimeUnit(TimeUnit.Millisecond) });
#else
BenchmarkRunner.Run<RunBenchmark>(DefaultConfig.Instance.WithSummaryStyle(SummaryStyle.Default.WithTimeUnit(TimeUnit.Millisecond)));
#endif
Console.WriteLine("Done");