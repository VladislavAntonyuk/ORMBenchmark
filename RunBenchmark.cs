using BenchmarkDotNet.Attributes;
using OrmBenchmark.AdoNet;
using OrmBenchmark.Dapper;
using OrmBenchmark.Database;
using OrmBenchmark.EfCore;
using OrmBenchmark.NHibernate;

namespace OrmBenchmark;

public class RunBenchmark
{
    public IEnumerable<IOperation> Operations()
    {
        yield return new AdoNetOperation();
        yield return new NHibernateOperation();
        yield return new EfCoreOperation();
        yield return new EfCoreNoTrackingOperation();
        yield return new EfCoreRawSqlOperation();
        yield return new DapperOperation();
    }

    [Benchmark]
    [ArgumentsSource(nameof(Operations))]
    public Task<Player> GetPlayerById(IOperation operation)
    {
        return operation.GetPlayerById(1);
    }

    [Benchmark]
    [ArgumentsSource(nameof(Operations))]
    public Task<List<Player>> GetPlayersForTeam(IOperation operation)
    {
        return operation.GetPlayersForTeam(1);
    }

    [Benchmark]
    [ArgumentsSource(nameof(Operations))]
    public Task<List<Team>> GetTeamsForSport(IOperation operation)
    {
        return operation.GetTeamsForSport(1);
    }

    [Benchmark]
    [ArgumentsSource(nameof(Operations))]
    public Task<List<Player>> GetPlayersForOlympiad(IOperation operation)
    {
        return operation.GetPlayersForOlympiad(1);
    }

    [Benchmark]
    [ArgumentsSource(nameof(Operations))]
    public Task<List<Player>> GetPlayersWithIncludeForOlympiad(IOperation operation)
    {
        return operation.GetPlayersWithIncludeForOlympiad(1);
    }

    [Benchmark]
    [ArgumentsSource(nameof(Operations))]
    public Task CreateOlympiad(IOperation operation)
    {
        return operation.CreateOlympiad();
    }

    [Benchmark]
    [ArgumentsSource(nameof(Operations))]
    public Task UpdateOlympiad(IOperation operation)
    {
        return operation.UpdateOlympiad();
    }
}