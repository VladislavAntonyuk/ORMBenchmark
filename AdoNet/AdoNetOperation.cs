using System.Configuration;
using System.Data.SqlClient;
using OrmBenchmark.Database;

namespace OrmBenchmark.AdoNet;

public class AdoNetOperation : IOperation
{
    public async Task<Player> GetPlayerById(int id)
    {
        var sqlCommand = await GetCommand();
        sqlCommand.Parameters.AddWithValue("id", id);
        sqlCommand.CommandText = "SELECT * FROM Players WHERE Id = @id";
        return await GetSingleResult(sqlCommand, ParsePlayer);
    }


    public async Task<List<Player>> GetPlayersForTeam(int teamId)
    {
        var sqlCommand = await GetCommand();
        sqlCommand.Parameters.AddWithValue("teamId", teamId);
        sqlCommand.CommandText = "SELECT * FROM Players WHERE TeamId = @teamId";
        return await GetList(sqlCommand, ParsePlayer);
    }

    public async Task<List<Team>> GetTeamsForSport(int sportId)
    {
        var teamsCommand = await GetCommand();
        teamsCommand.Parameters.AddWithValue("sportId", sportId);
        teamsCommand.CommandText = "SELECT * FROM Teams WHERE SportId = @sportId";
        var teams = await GetDictionary(teamsCommand, ParseTeam, team => team.Id);

        var playersCommand = await GetCommand();
        playersCommand.Parameters.AddWithValue("sportId", sportId);
        playersCommand.CommandText = "SELECT [p].* FROM [Players] AS [p] " +
                                     "INNER JOIN [Teams] AS [t] ON [p].[TeamId] = [t].[Id] " +
                                     "WHERE t.SportId = @sportId";
        await foreach (var player in ExecuteAndParseResults(playersCommand, ParsePlayer))
        {
            var team = teams[player.TeamId];
            team.Players.Add(player);
            player.Team = team;
        }

        return teams.Values.ToList();
    }

    public async Task<List<Player>> GetPlayersForOlympiad(int olympiadId)
    {
        var sqlCommand = await GetCommand();
        sqlCommand.Parameters.AddWithValue("olympiadId", olympiadId);
        sqlCommand.CommandText = "SELECT [p].* FROM [Players] AS [p] " +
                                 "INNER JOIN [Teams] AS [t] ON [p].[TeamId] = [t].[Id] " +
                                 "INNER JOIN [Sports] AS [s] ON [t].[SportId] = [s].[Id] " +
                                 "INNER JOIN [Olympics] AS [o] ON [s].[OlympiadId] = [o].[Id] " +
                                 "WHERE [o].[Id] = @olympiadId";
        return await GetList(sqlCommand, ParsePlayer);
    }

    public async Task<List<Player>> GetPlayersWithIncludeForOlympiad(int olympiadId)
    {
        var sqlCommand = await GetCommand();
        sqlCommand.Parameters.AddWithValue("olympiadId", olympiadId);
        sqlCommand.CommandText = "SELECT * FROM Olympics WHERE Id = @olympiadId";
        var olympiad = await GetSingleResult(sqlCommand, ParseOlympiad);

        var sportsCommand = await GetCommand();
        sportsCommand.Parameters.AddWithValue("olympiadId", olympiadId);
        sportsCommand.CommandText = "SELECT * FROM Sports WHERE OlympiadId = @olympiadId";
        var sports = await GetDictionary(sportsCommand, ParseSport, sport => sport.Id);
        foreach (var sport in sports.Values)
        {
            olympiad.Sports.Add(sport);
            sport.Olympiad = olympiad;
        }

        var teamsCommand = await GetCommand();
        teamsCommand.Parameters.AddWithValue("olympiadId", olympiadId);
        teamsCommand.CommandText = "SELECT * FROM Teams AS [t] " +
                                   "INNER JOIN [Sports] AS [s] ON [t].[SportId] = [s].[Id] " +
                                   "WHERE [s].[OlympiadId] = @olympiadId";
        var teams = await GetDictionary(teamsCommand, ParseTeam, team => team.Id);
        foreach (var team in teams.Values)
        {
            var sport = sports[team.SportId];
            sport.Teams.Add(team);
            team.Sport = sport;
        }

        var players = await GetPlayersForOlympiad(olympiadId);
        foreach (var player in players)
        {
            var team = teams[player.TeamId];
            team.Players.Add(player);
            player.Team = team;
        }

        return players;
    }

    public async Task CreateOlympiad()
    {
        var sqlCommand = await GetCommand();
        var olympiad = new Olympiad
        {
            Sports = new List<Sport>(),
            Start = new DateTime(Random.Shared.Next(1980, 2022), 01, 01),
            End = new DateTime(Random.Shared.Next(1980, 2022), 01, 01)
        };
        sqlCommand.Parameters.AddWithValue("city", olympiad.City);
        // TODO: figure out how to pass parameter value as datetime2
        sqlCommand.Parameters.AddWithValue("start", olympiad.Start);
        sqlCommand.Parameters.AddWithValue("end", olympiad.End);
        sqlCommand.CommandText =
            "INSERT INTO Olympics (City, Start, [End]) OUTPUT Inserted.Id VALUES (@city, @start, @end)";
        var insertedId = await sqlCommand.ExecuteScalarAsync();
        if (insertedId == null) throw new ArgumentException("insert failed");
        olympiad.Id = (int)insertedId;
        var sport = await CreateSport(olympiad);
        olympiad.Sports.Add(sport);
    }

    public async Task UpdateOlympiad()
    {
        var getOlympiadCommand = await GetCommand();
        getOlympiadCommand.Parameters.AddWithValue("olympiadId", Random.Shared.Next(1, 10));
        getOlympiadCommand.CommandText = "SELECT * FROM Olympics WHERE id = @olympiadId";
        var olympiad = await GetSingleResult(getOlympiadCommand, ParseOlympiad);
        olympiad.Start = new DateTime(Random.Shared.Next(1980, 2022), 01, 01);

        var updateCommand = await GetCommand();
        updateCommand.Parameters.AddWithValue("city", olympiad.City);
        updateCommand.Parameters.AddWithValue("start", olympiad.Start);
        updateCommand.Parameters.AddWithValue("end", olympiad.End);
        updateCommand.Parameters.AddWithValue("olympiadId", olympiad.Id);
        updateCommand.CommandText =
            "UPDATE Olympics SET City = @city, Start = @start, [End] = @end WHERE Id = @olympiadId";
        await updateCommand.ExecuteNonQueryAsync();

        var sport = await CreateSport(olympiad);
        olympiad.Sports.Add(sport);
    }

    public async Task DeleteOlympiad()
    {
        var sqlCommand = await GetCommand();
        sqlCommand.Parameters.AddWithValue("olympiadId", Random.Shared.Next(1, 10));
        sqlCommand.CommandText = "DELETE FROM Olympiad WHERE Id = @olympiadId";
        await sqlCommand.ExecuteNonQueryAsync();
    }

    public override string ToString()
    {
        return nameof(AdoNetOperation);
    }

    private async Task<Sport> CreateSport(Olympiad olympiad)
    {
        var sqlCommand = await GetCommand();
        var sport = new Sport
        {
            Teams = new List<Team>(),
            OlympiadId = olympiad.Id,
            Olympiad = olympiad
        };
        sqlCommand.Parameters.AddWithValue("name", sport.Name);
        sqlCommand.Parameters.AddWithValue("olympiadId", sport.Olympiad.Id);
        sqlCommand.CommandText = "INSERT INTO Sports (Name, OlympiadId) OUTPUT Inserted.Id VALUES (@name, @olympiadId)";
        var insertedId = await sqlCommand.ExecuteScalarAsync();
        if (insertedId == null) throw new ArgumentException("insert failed");
        sport.Id = (int)insertedId;
        var team = await CreateTeam(sport);
        sport.Teams.Add(team);
        return sport;
    }

    private async Task<Team> CreateTeam(Sport sport)
    {
        var sqlCommand = await GetCommand();
        var team = new Team
        {
            Players = new List<Player>(),
            Sport = sport,
            SportId = sport.Id
        };
        sqlCommand.Parameters.AddWithValue("name", team.Name);
        sqlCommand.Parameters.AddWithValue("sportId", team.Sport.Id);
        sqlCommand.Parameters.AddWithValue("foundation", team.Foundation);
        sqlCommand.CommandText =
            "INSERT INTO Teams (Name, Foundation, SportId) OUTPUT Inserted.Id VALUES (@name, @foundation, @sportId)";
        var insertedId = await sqlCommand.ExecuteScalarAsync();
        if (insertedId == null) throw new ArgumentException("insert failed");
        team.Id = (int)insertedId;
        var player = await CreatePlayer(team);
        team.Players.Add(player);
        return team;
    }


    private async Task<Player> CreatePlayer(Team team)
    {
        var sqlCommand = await GetCommand();
        var player = new Player
        {
            Team = team,
            TeamId = team.Id
        };
        sqlCommand.Parameters.AddWithValue("firstName", player.FirstName);
        sqlCommand.Parameters.AddWithValue("lastName", player.LastName);
        sqlCommand.Parameters.AddWithValue("teamId", player.Team.Id);
        sqlCommand.Parameters.AddWithValue("birthday", player.Birthday);
        sqlCommand.CommandText =
            "INSERT INTO Teams (FirstName, LastName, Birthday, TeamId) OUTPUT Inserted.Id VALUES (@firstName, @lastName, @birthday, @teamId)";
        var insertedId = await sqlCommand.ExecuteScalarAsync();
        if (insertedId == null) throw new ArgumentException("insert failed");
        player.Id = (int)insertedId;
        return player;
    }

    private static async Task<SqlCommand> GetCommand()
    {
        var connectionString = ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString;
        var sqlConnection = new SqlConnection(connectionString);
        await sqlConnection.OpenAsync();
        var sqlCommand = sqlConnection.CreateCommand();
        return sqlCommand;
    }

    private static async IAsyncEnumerable<T> ExecuteAndParseResults<T>(SqlCommand sqlCommand,
        Func<SqlDataReader, T> parser)
    {
        var sqlDataReader = await sqlCommand.ExecuteReaderAsync();
        while (await sqlDataReader.ReadAsync()) yield return parser(sqlDataReader);
    }

    private static async Task<T> GetSingleResult<T>(SqlCommand sqlCommand, Func<SqlDataReader, T> parser)
        where T : class
    {
        await foreach (var result in ExecuteAndParseResults(sqlCommand, parser)) return result;

        return null;
    }

    private static async Task<List<T>> GetList<T>(SqlCommand sqlCommand, Func<SqlDataReader, T> parser)
    {
        var results = new List<T>();
        await foreach (var result in ExecuteAndParseResults(sqlCommand, parser)) results.Add(result);

        return results;
    }

    private static async Task<Dictionary<T, TV>> GetDictionary<T, TV>(SqlCommand sqlCommand,
        Func<SqlDataReader, TV> parser, Func<TV, T> keyExtractor)
    {
        var results = new Dictionary<T, TV>();
        await foreach (var result in ExecuteAndParseResults(sqlCommand, parser))
            results.Add(keyExtractor(result), result);

        return results;
    }


    private static Player ParsePlayer(SqlDataReader sqlDataReader)
    {
        return new()
        {
            Id = (int)sqlDataReader[nameof(Player.Id)],
            Birthday = (DateTime)sqlDataReader[nameof(Player.Birthday)],
            FirstName = GetStringOrNull(sqlDataReader, nameof(Player.FirstName)),
            LastName = GetStringOrNull(sqlDataReader, nameof(Player.LastName)),
            TeamId = (int)sqlDataReader[nameof(Player.TeamId)]
        };
    }

    private static Team ParseTeam(SqlDataReader sqlDataReader)
    {
        return new()
        {
            Id = (int)sqlDataReader[nameof(Team.Id)],
            Foundation = (DateTime)sqlDataReader[nameof(Team.Foundation)],
            Name = GetStringOrNull(sqlDataReader, nameof(Team.Name)),
            SportId = (int)sqlDataReader[nameof(Team.SportId)],
            Players = new List<Player>()
        };
    }

    private static Sport ParseSport(SqlDataReader sqlDataReader)
    {
        return new()
        {
            Id = (int)sqlDataReader[nameof(Sport.Id)],
            Name = GetStringOrNull(sqlDataReader, nameof(Sport.Name)),
            OlympiadId = (int)sqlDataReader[nameof(Sport.OlympiadId)],
            Teams = new List<Team>()
        };
    }

    private static Olympiad ParseOlympiad(SqlDataReader sqlDataReader)
    {
        return new()
        {
            Id = (int)sqlDataReader[nameof(Olympiad.Id)],
            Start = (DateTime)sqlDataReader[nameof(Olympiad.Start)],
            End = (DateTime)sqlDataReader[nameof(Olympiad.End)],
            City = GetStringOrNull(sqlDataReader, nameof(Olympiad.City)),
            Sports = new List<Sport>()
        };
    }

    private static string GetStringOrNull(SqlDataReader sqlDataReader, string parameterName)
    {
        return sqlDataReader[parameterName] == DBNull.Value ? null : (string)sqlDataReader[parameterName];
    }
}