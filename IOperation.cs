using OrmBenchmark.Database;

namespace OrmBenchmark;

public interface IOperation
{
    Task<Player> GetPlayerById(int id);
    Task<List<Player>> GetPlayersForTeam(int teamId);
    Task<List<Team>> GetTeamsForSport(int sportId);
    Task<List<Player>> GetPlayersForOlympiad(int olympiadId);
    Task<List<Player>> GetPlayersWithIncludeForOlympiad(int olympiadId);

    Task CreateOlympiad();
    Task UpdateOlympiad();
    Task DeleteOlympiad();
}