using System.Configuration;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Mapping;
using NHibernate;
using NHibernate.Linq;
using OrmBenchmark.Database;

namespace OrmBenchmark.NHibernate;

public class NHibernateOperation : IOperation
{
    private static readonly ISessionFactory SessionFactory;

    static NHibernateOperation()
    {
        var sessionFactory = Fluently.Configure()
            .Database(MsSqlConfiguration.MsSql2012.ConnectionString(
                cs => cs.Is(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString)))
            .Mappings(m =>
                m.FluentMappings.AddFromAssemblyOf<NHibernateOperation>())
            .BuildSessionFactory();
        SessionFactory = sessionFactory;
    }

    public async Task<Player> GetPlayerById(int id)
    {
        using var conn = GetConnection();
        return await conn.GetAsync<Player>(id);
    }

    public async Task<List<Player>> GetPlayersForTeam(int teamId)
    {
        using var conn = GetConnection();
        return await conn.Query<Player>()
            .Where(p => p.Team.Id == teamId).ToListAsync();
    }

    public async Task<List<Team>> GetTeamsForSport(int sportId)
    {
        using var conn = GetConnection();
        return await conn.Query<Team>()
            .Where(t => t.Sport.Id == sportId)
            .Fetch(t => t.Players).ToListAsync();
    }

    public async Task<List<Player>> GetPlayersForOlympiad(int olympiadId)
    {
        using var conn = GetConnection();
        return await conn.Query<Player>()
            .Where(p => p.Team.Sport.Olympiad.Id == olympiadId)
            .ToListAsync();
    }

    public async Task<List<Player>> GetPlayersWithIncludeForOlympiad(int olympiadId)
    {
        using var conn = GetConnection();
        return await conn.Query<Player>()
            .Where(p => p.Team.Sport.Olympiad.Id == olympiadId)
            .Fetch(p => p.Team)
            .ThenFetch(t => t.Sport)
            .ThenFetch(s => s.Olympiad).ToListAsync();
    }

    public async Task CreateOlympiad()
    {
        using var conn = GetConnection();
        using var transaction = conn.BeginTransaction();
        var olympiad = CreateOlympiadObject();
        await conn.SaveAsync(olympiad);
        await conn.FlushAsync();
        await transaction.CommitAsync();
    }

    public async Task UpdateOlympiad()
    {
        using var conn = GetConnection();
        using var transaction = conn.BeginTransaction();
        var olympiad = await conn.GetAsync<Olympiad>(Random.Shared.Next(1, 10));
        if (olympiad != null)
        {
            var sport = CreateSport();
            olympiad.Sports.Add(sport);
            sport.Olympiad = olympiad;
            olympiad.Start = new DateTime(Random.Shared.Next(1980, 2022), 01, 01);
            await conn.UpdateAsync(olympiad);
        }

        await conn.FlushAsync();
        await transaction.CommitAsync();
    }

    public async Task DeleteOlympiad()
    {
        using var conn = GetConnection();
        using var transaction = conn.BeginTransaction();
        var olympiad = await conn.GetAsync<Olympiad>(Random.Shared.Next(1, 10));
        await conn.DeleteAsync(olympiad);
        await conn.FlushAsync();
        await transaction.CommitAsync();
    }

    public override string ToString()
    {
        return nameof(NHibernateOperation);
    }

    private static Olympiad CreateOlympiadObject()
    {
        var sport = CreateSport();
        var olympiad = new Olympiad
        {
            Sports = new List<Sport>
            {
                sport
            }
        };
        sport.Olympiad = olympiad;
        return olympiad;
    }

    private static Sport CreateSport()
    {
        var team = CreateTeam();
        var sport = new Sport
        {
            Teams = new List<Team>
            {
                team
            }
        };
        team.Sport = sport;
        return sport;
    }

    private static Team CreateTeam()
    {
        var player = new Player();
        var team = new Team
        {
            Players = new List<Player>
            {
                player
            }
        };
        player.Team = team;
        return team;
    }

    private ISession GetConnection()
    {
        return SessionFactory.OpenSession();
    }
}

public class OlympiadMapping : ClassMap<Olympiad>
{
    public OlympiadMapping()
    {
        Table("Olympics");
        Id(x => x.Id).GeneratedBy.Native();
        Map(x => x.City);
        Map(x => x.Start);
        Map(x => x.End)
            .Column("[End]");
        HasMany(x => x.Sports)
            .Inverse()
            .Cascade.All()
            .KeyColumn(nameof(Sport.OlympiadId));
    }
}

public class SportMapping : ClassMap<Sport>
{
    public SportMapping()
    {
        Table("Sports");
        Id(x => x.Id).GeneratedBy.Native();
        Map(x => x.Name);
        References(x => x.Olympiad)
            .Column(nameof(Sport.OlympiadId));
        HasMany(x => x.Teams)
            .Inverse()
            .Cascade.All()
            .KeyColumn(nameof(Team.SportId));
    }
}

public class TeamMapping : ClassMap<Team>
{
    public TeamMapping()
    {
        Table("Teams");
        Id(x => x.Id).GeneratedBy.Native();
        Map(x => x.Name);
        Map(x => x.Foundation);
        References(x => x.Sport)
            .Column(nameof(Team.SportId));
        HasMany(x => x.Players)
            .Inverse()
            .Cascade.All()
            .KeyColumn(nameof(Player.TeamId));
    }
}

public class PlayerMapping : ClassMap<Player>
{
    public PlayerMapping()
    {
        Table("Players");
        Id(x => x.Id).GeneratedBy.Native();
        Map(x => x.FirstName);
        Map(x => x.LastName);
        Map(x => x.Birthday);
        References(x => x.Team)
            .Column(nameof(Player.TeamId));
    }
}