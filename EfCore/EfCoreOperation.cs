using Microsoft.EntityFrameworkCore;
using OrmBenchmark.Database;

namespace OrmBenchmark.EfCore;

public class EfCoreOperation : IOperation
{
    public async Task<Player> GetPlayerById(int id)
    {
        await using var context = new OlympiadContext();
        return await context.Players.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Player>> GetPlayersForTeam(int teamId)
    {
        await using var context = new OlympiadContext();
        return await context.Players
            .Where(x => x.TeamId == teamId)
            .ToListAsync();
    }

    public async Task<List<Team>> GetTeamsForSport(int sportId)
    {
        await using var context = new OlympiadContext();
        return await context.Teams
            .Include(x => x.Players)
            .Where(x => x.SportId == sportId)
            .ToListAsync();
    }

    public async Task<List<Player>> GetPlayersForOlympiad(int olympiadId)
    {
        await using var context = new OlympiadContext();
        return await context.Players
            .Where(x => x.Team.Sport.Olympiad.Id == olympiadId)
            .ToListAsync();
    }

    public async Task<List<Player>> GetPlayersWithIncludeForOlympiad(int olympiadId)
    {
        await using var context = new OlympiadContext();
        return await context.Players
            .Include(x => x.Team)
            .ThenInclude(x => x.Sport)
            .ThenInclude(x => x.Olympiad)
            .Where(x => x.Team.Sport.Olympiad.Id == olympiadId)
            .ToListAsync();
    }

    public async Task CreateOlympiad()
    {
        await using var context = new OlympiadContext();
        var olympiad = new Olympiad();
        await context.AddAsync(olympiad);
        var sport = new Sport
        {
            Olympiad = olympiad
        };
        await context.AddAsync(sport);
        var team = new Team
        {
            Sport = sport,
            Foundation = new DateTime(1979, 01, 01)
        };
        await context.AddAsync(team);
        var player = new Player
        {
            Team = team,
            Birthday = new DateTime(1979, 01, 01)
        };
        await context.AddAsync(player);
        await context.SaveChangesAsync();
    }

    public async Task UpdateOlympiad()
    {
        await using var context = new OlympiadContext();
        var olympiad = await context.Olympics.FindAsync(Random.Shared.Next(1, 10));
        if (olympiad != null)
        {
            var sport = new Sport
            {
                Olympiad = olympiad
            };
            await context.AddAsync(sport);
            var team = new Team
            {
                Sport = sport,
                Foundation = new DateTime(1979, 01, 01)
            };
            await context.AddAsync(team);
            var player = new Player
            {
                Team = team,
                Birthday = new DateTime(1979, 01, 01)
            };
            await context.AddAsync(player);
            olympiad.Start = new DateTime(Random.Shared.Next(1980, 2022), 01, 01);
            context.Olympics.Update(olympiad);
            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteOlympiad()
    {
        await using var context = new OlympiadContext();
        var olympiad = await context.Olympics.FindAsync(Random.Shared.Next(1, 10));
        if (olympiad != null)
        {
            context.Olympics.Remove(olympiad);
            await context.SaveChangesAsync();
        }
    }

    public override string ToString()
    {
        return nameof(EfCoreOperation);
    }
}