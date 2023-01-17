using Microsoft.EntityFrameworkCore;
using OrmBenchmark.Database;

namespace OrmBenchmark.EfCore;

public class EfCoreRawSqlOperation : IOperation
{
    public async Task<Player> GetPlayerById(int id)
    {
        await using var context = new OlympiadContext();
        return await context.Players.FromSqlInterpolated($"SELECT * FROM Players WHERE id = {id}")
            .FirstOrDefaultAsync();
    }

    public async Task<List<Player>> GetPlayersForTeam(int teamId)
    {
        await using var context = new OlympiadContext();
        return await context.Players.FromSqlInterpolated($"SELECT * FROM Players WHERE TeamId = {teamId}")
            .ToListAsync();
    }

    public async Task<List<Team>> GetTeamsForSport(int sportId)
    {
        await using var context = new OlympiadContext();
        return await context.Teams.FromSqlInterpolated(
                //$"SELECT Teams.* FROM Teams LEFT JOIN Players ON Teams.Id = Players.TeamId WHERE SportId = { sportId }")
                $"SELECT Teams.* FROM Teams WHERE SportId = {sportId}")
            .Include(x => x.Players)
            .ToListAsync();
    }

    public async Task<List<Player>> GetPlayersForOlympiad(int olympiadId)
    {
        await using var context = new OlympiadContext();
        return await context.Players.FromSqlRaw(
            "SELECT [p].* FROM [Players] AS [p] " +
            "INNER JOIN [Teams] AS [t] ON [p].[TeamId] = [t].[Id] " +
            "INNER JOIN [Sports] AS [s] ON [t].[SportId] = [s].[Id] " +
            "INNER JOIN [Olympics] AS [o] ON [s].[OlympiadId] = [o].[Id] " +
            "WHERE [o].[Id] = {0}", olympiadId).ToListAsync();
    }

    public async Task<List<Player>> GetPlayersWithIncludeForOlympiad(int olympiadId)
    {
        await using var context = new OlympiadContext();
        return await context.Players.FromSqlRaw(
                "SELECT [p].[Id], [p].[Birthday], [p].[FirstName], [p].[LastName], [p].[TeamId] FROM [Players] AS [p]")
            .Include(x => x.Team)
            .ThenInclude(x => x.Sport)
            .ThenInclude(x => x.Olympiad)
            .Where(x => x.Team.Sport.Olympiad.Id == olympiadId).ToListAsync();
    }

    public async Task CreateOlympiad()
    {
        await using var context = new OlympiadContext();
        var sql = $@"
                    INSERT INTO [Olympics] ([City], [Start], [End])
                    VALUES ('test', '{new DateTime(1984, 01, 01):dd/MM/yyyy}','{new DateTime(1984, 02, 01):dd/MM/yyyy}');

                    INSERT INTO[Sports]([OlympiadId])
                    VALUES(SCOPE_IDENTITY());

                    INSERT INTO [Teams] ([SportId], [Foundation])
                    VALUES (SCOPE_IDENTITY(), '{new DateTime(1979, 01, 01):dd/MM/yyyy}');

                    INSERT INTO [Players] ([TeamId], [Birthday])
                    VALUES (SCOPE_IDENTITY(), '{new DateTime(1979, 01, 01):dd/MM/yyyy}');";
        await context.Database.ExecuteSqlRawAsync(sql);
    }

    public async Task UpdateOlympiad()
    {
        var olympiadId = Random.Shared.Next(1, 10);
        await using var context = new OlympiadContext();
        await context.Database.ExecuteSqlInterpolatedAsync($@"
                    Update [Olympics] SET Start={new DateTime(Random.Shared.Next(1980, 2022), 01, 01)} WHERE Id={olympiadId}");
        var sql = $@"
                    INSERT INTO[Sports]([OlympiadId])
                    VALUES({olympiadId});

                    INSERT INTO [Teams] ([SportId], [Foundation])
                    VALUES (SCOPE_IDENTITY(), '{new DateTime(1979, 01, 01):dd/MM/yyyy}');

                    INSERT INTO [Players] ([TeamId], [Birthday])
                    VALUES (SCOPE_IDENTITY(), '{new DateTime(1979, 01, 01):dd/MM/yyyy}');";
        await context.Database.ExecuteSqlRawAsync(sql);
    }

    public async Task DeleteOlympiad()
    {
        await using var context = new OlympiadContext();
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE FROM Olympics WHERE id={Random.Shared.Next(1, 10)}");
    }

    public override string ToString()
    {
        return nameof(EfCoreRawSqlOperation);
    }
}