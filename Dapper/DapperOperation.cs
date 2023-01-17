using Dapper;
using Microsoft.EntityFrameworkCore;
using OrmBenchmark.Database;
using OrmBenchmark.EfCore;

namespace OrmBenchmark.Dapper;

public class DapperOperation : IOperation
{
    public async Task<Player> GetPlayerById(int id)
    {
        await using var context = new OlympiadContext().Database.GetDbConnection();
        return await context.QueryFirstOrDefaultAsync<Player>("SELECT * FROM Players WHERE id = @id", new { id });
    }

    public async Task<List<Player>> GetPlayersForTeam(int teamId)
    {
        await using var context = new OlympiadContext().Database.GetDbConnection();
        var result = await context.QueryAsync<Player>("SELECT * FROM Players WHERE TeamId = @teamId", new { teamId });
        return result.ToList();
    }

    public async Task<List<Team>> GetTeamsForSport(int sportId)
    {
        await using var context = new OlympiadContext().Database.GetDbConnection();
        var teamDictionary = new Dictionary<int, Team>();

        var result = await context.QueryAsync<Team, Player, Team>(
            "SELECT Teams.*, Players.* FROM Teams LEFT JOIN Players ON Teams.Id = Players.TeamId WHERE SportId = @sportId",
            (team, player) =>
            {
                if (!teamDictionary.TryGetValue(team.Id, out var teamEntry))
                {
                    teamEntry = team;
                    teamEntry.Players = new List<Player>();
                    teamDictionary.Add(teamEntry.Id, teamEntry);
                }

                teamEntry.Players.Add(player);
                return teamEntry;
            }, new { sportId });
        return result.Distinct().ToList();
    }

    public async Task<List<Player>> GetPlayersForOlympiad(int olympiadId)
    {
        await using var context = new OlympiadContext().Database.GetDbConnection();
        var result = await context.QueryAsync<Player>(
            "SELECT [p].* FROM [Players] AS [p] " +
            "INNER JOIN [Teams] AS [t] ON [p].[TeamId] = [t].[Id] " +
            "INNER JOIN [Sports] AS [s] ON [t].[SportId] = [s].[Id] " +
            "INNER JOIN [Olympics] AS [o] ON [s].[OlympiadId] = [o].[Id] " +
            "WHERE [o].[Id] = @olympiadId",
            new { olympiadId });
        return result.ToList();
    }

    public async Task<List<Player>> GetPlayersWithIncludeForOlympiad(int olympiadId)
    {
        await using var context = new OlympiadContext().Database.GetDbConnection();
        var result = await context.QueryAsync<Player, Team, Sport, Olympiad, Player>(
            "SELECT [p].[Id], [p].[Birthday], [p].[FirstName], [p].[LastName], [p].[TeamId], [t].[Id], [t].[Foundation], [t].[Name], [t].[SportId], [s].[Id], [s].[Name], [s].[OlympiadId], [o].[Id], [o].[City], [o].[End], [o].[Start] FROM [Players] AS [p] " +
            "INNER JOIN [Teams] AS [t] ON [p].[TeamId] = [t].[Id] " +
            "INNER JOIN [Sports] AS [s] ON [t].[SportId] = [s].[Id] " +
            "INNER JOIN [Olympics] AS [o] ON [s].[OlympiadId] = [o].[Id] " +
            "WHERE [o].[Id] = @olympiadId",
            (player, _, _, _) => player,
            new { olympiadId });
        return result.ToList();
    }

    public async Task CreateOlympiad()
    {
        await using var context = new OlympiadContext().Database.GetDbConnection();
        var olympiadId = await context.QuerySingleAsync<int>(@"
                    INSERT INTO [Olympics] ([City], [Start], [End])
                    OUTPUT INSERTED.Id
                    VALUES (@City, @Start, @End);",
            new { City = "test", Start = new DateTime(1984, 01, 01), End = new DateTime(1984, 02, 01) });
        var sportId = await context.QuerySingleAsync<int>(@"
                    INSERT INTO [Sports] ([OlympiadId])
                    OUTPUT INSERTED.Id
                    VALUES (@OlympiadId);", new { OlympiadId = olympiadId });
        var teamId = await context.QuerySingleAsync<int>(@"
                    INSERT INTO [Teams] ([SportId], [Foundation])
                    OUTPUT INSERTED.Id
                    VALUES (@SportId, @Foundation);",
            new { SportId = sportId, Foundation = new DateTime(1979, 01, 01) });
        await context.QuerySingleAsync<int>(@"
                    INSERT INTO [Players] ([TeamId], [Birthday])
                    OUTPUT INSERTED.Id
                    VALUES (@TeamId, @Birthday);", new { TeamId = teamId, Birthday = new DateTime(1979, 01, 01) });
    }

    public async Task UpdateOlympiad()
    {
        await using var context = new OlympiadContext().Database.GetDbConnection();
        var olympiadId = Random.Shared.Next(1, 10);

        await context.ExecuteAsync(@"
                    Update [Olympics] SET Start=@start WHERE Id=@id",
            new { start = new DateTime(Random.Shared.Next(1980, 2022), 01, 01), id = olympiadId });
        var sportId = await context.QuerySingleAsync<int>(@"
                    INSERT INTO [Sports] ([OlympiadId])
                    OUTPUT INSERTED.Id
                    VALUES (@OlympiadId);", new { OlympiadId = olympiadId });
        var teamId = await context.QuerySingleAsync<int>(@"
                    INSERT INTO [Teams] ([SportId], [Foundation])
                    OUTPUT INSERTED.Id
                    VALUES (@SportId, @Foundation);",
            new { SportId = sportId, Foundation = new DateTime(1979, 01, 01) });
        await context.QuerySingleAsync<int>(@"
                    INSERT INTO [Players] ([TeamId], [Birthday])
                    OUTPUT INSERTED.Id
                    VALUES (@TeamId, @Birthday);", new { TeamId = teamId, Birthday = new DateTime(1979, 01, 01) });
    }

    public async Task DeleteOlympiad()
    {
        await using var context = new OlympiadContext().Database.GetDbConnection();
        await context.ExecuteAsync("DELETE FROM Olympics WHERE id=@id", new { id = Random.Shared.Next(1, 10) });
    }

    public override string ToString()
    {
        return nameof(DapperOperation);
    }
}