using System.Diagnostics;
using OrmBenchmark.Database;
using OrmBenchmark.EfCore;

namespace OrmBenchmark;

public static class Generator
{
    public static List<string> GetFirstNames()
    {
        return new List<string>
        {
            "Aaron",
            "James",
            "John",
            "Matthew",
            "Michael",
            "William",
            "David",
            "Luis",
            "Vincent",
            "Paul",
            "Mark",
            "Steven",
            "Edward",
            "Brian",
            "Christopher",
            "Mary",
            "Patricia",
            "Linda",
            "Barbara",
            "Kay lee",
            "Mackenzie",
            "Karen",
            "Nancy",
            "Ashley",
            "Jennifer",
            "Jessica",
            "Michelle",
            "Kimberly",
            "Maria",
            "Gretchen"
        };
    }

    public static List<string> GetLastNames()
    {
        return new List<string>
        {
            "Smith",
            "Johnson",
            "Jones",
            "Williams",
            "Brown",
            "Miller",
            "Davis",
            "Garcia",
            "Rodriguez",
            "Wilson",
            "Martinez",
            "Anderson",
            "Taylor",
            "Thomas",
            "Moore",
            "Lee",
            "Gonzalez",
            "Harris",
            "Clark",
            "Lewis",
            "Robinson",
            "Walker",
            "Perez",
            "Hall",
            "Sanchez",
            "Wright",
            "White",
            "Chekhovian",
            "Morris",
            "Nguyen",
            "Edwards",
            "Murphy",
            "Rivera",
            "Baker",
            "Adams",
            "Carter",
            "Phillips",
            "Torres",
            "King",
            "Scott",
            "Davies",
            "Torrance",
            "Graham",
            "L eighty",
            "Jackson"
        };
    }

    public static List<string> GetCityNames()
    {
        return new List<string>
        {
            "New York",
            "Los Angles",
            "Chicago",
            "Houston",
            "Philadelphia",
            "Phoenix",
            "San Francisco",
            "San Diego",
            "Dallas",
            "San Antonio",
            "Seattle",
            "Portland",
            "San Jose",
            "Nashville",
            "Indianapolis",
            "New Orleans",
            "Minneapolis",
            "Boston",
            "Toronto",
            "Washington",
            "Baltimore",
            "Charlotte",
            "Atlanta",
            "Miami",
            "Jacksonville",
            "Tampa Bay",
            "Milwaukee",
            "Detroit",
            "St Louis",
            "Kansas City"
        };
    }

    public static List<string> GetTeamNames()
    {
        return new List<string>
        {
            "Panthers",
            "Cougars",
            "Lions",
            "Bears",
            "Minutemen",
            "Raptors",
            "Cardinals",
            "Lightning",
            "Thunder",
            "Hurricanes",
            "Bison",
            "Devils",
            "Pterodactyls",
            "Rockers",
            "Canes",
            "Knights",
            "Waves",
            "Hangers",
            "Bombers",
            "Wizards",
            "Brawlers",
            "Volunteers",
            "Hawks",
            "Thrashers",
            "Snakes",
            "Venom",
            "Liberty",
            "Warriors",
            "Sparks",
            "Huskies",
            "Penguins",
            "Cheetahs",
            "Moose",
            "Sabers",
            "Mercenaries"
        };
    }

    public static List<string> GetSportNames()
    {
        return new List<string>
        {
            "Baseball",
            "American Football",
            "Association Football",
            "Rugby",
            "Basketball",
            "Ice Hockey",
            "Lacrosse",
            "Cricket",
            "Curling",
            "Field Hockey",
            "Quid ditch",
            "Track & Field"
        };
    }

    public static async Task SeedData(bool clean = false)
    {
        await using var context = new OlympiadContext();
        if (clean)
        {
            await context.Database.EnsureDeletedAsync();
            Console.WriteLine("Database is deleted.");
        }

        var isCreated = await context.Database.EnsureCreatedAsync();
        if (!isCreated)
        {
            Console.WriteLine("Database is not created.");
            return;
        }

        var stopWatch = new Stopwatch();
        stopWatch.Start();
        var olympics = GenerateOlympics(10);
        await context.Olympics.AddRangeAsync(olympics);

        var sports = new List<Sport>();
        foreach (var olympiad in olympics) sports.AddRange(GenerateSports(olympiad, 10));

        await context.Sports.AddRangeAsync(sports);

        var teams = new List<Team>();
        foreach (var sport in sports) teams.AddRange(GenerateTeams(sport, 100));

        await context.Teams.AddRangeAsync(teams);

        var players = new List<Player>();
        foreach (var team in teams) players.AddRange(GeneratePlayers(team, 100));

        await context.Players.AddRangeAsync(players);
        stopWatch.Stop();
        Console.WriteLine(
            $"Generated {olympics.Count} olympics, {sports.Count} sports, {teams.Count} teams, {players.Count} players. Elapsed time: {stopWatch.Elapsed}");
        stopWatch.Restart();
        await context.SaveChangesAsync();
        stopWatch.Stop();
        Console.WriteLine($"Data stored in db. Elapsed time: {stopWatch.Elapsed}");
    }

    public static List<Player> GeneratePlayers(Team team, int count)
    {
        var players = new List<Player>();

        var allFirstNames = GetFirstNames();
        var allLastNames = GetLastNames();
        var rand = new Random();
        var start = new DateTime(1975, 1, 1);
        var end = new DateTime(1998, 1, 1);

        for (var i = 0; i < count; i++)
        {
            var player = new Player();
            var newFirst = rand.Next(0, allFirstNames.Count - 1);
            player.FirstName = allFirstNames[newFirst];
            var newLast = rand.Next(0, allLastNames.Count - 1);
            player.LastName = allLastNames[newLast];
            player.Birthday = RandomDay(rand, start, end);
            player.Team = team;
            players.Add(player);
        }

        return players;
    }

    public static List<Team> GenerateTeams(Sport sport, int count)
    {
        var teams = new List<Team>();

        var allCityNames = GetCityNames();
        var allTeamNames = GetTeamNames();
        var rand = new Random();
        var start = new DateTime(1900, 1, 1);
        var end = new DateTime(2010, 1, 1);

        for (var i = 0; i < count; i++)
        {
            var team = new Team();
            var newCity = rand.Next(0, allCityNames.Count - 1);
            var newTeam = rand.Next(0, allTeamNames.Count - 1);
            team.Name = allCityNames[newCity] + " " + allTeamNames[newTeam];
            team.Foundation = RandomDay(rand, start, end);
            team.Sport = sport;
            teams.Add(team);
        }

        return teams;
    }

    public static List<Sport> GenerateSports(Olympiad olympiad, int count)
    {
        var sports = new List<Sport>();
        var allSportNames = GetSportNames();
        var rand = new Random();

        for (var i = 0; i < count; i++)
        {
            var newSport = rand.Next(0, allSportNames.Count - 1);
            sports.Add(new Sport
            {
                Olympiad = olympiad,
                Name = allSportNames[newSport]
            });
        }

        return sports;
    }

    public static List<Olympiad> GenerateOlympics(int count)
    {
        var olympics = new List<Olympiad>();
        var rand = new Random();
        var startDate = new DateTime(1980, 01, 01);
        var allCityNames = GetCityNames();
        for (var i = 0; i < count; i++)
        {
            startDate += TimeSpan.FromDays(365 * 4);
            olympics.Add(new Olympiad
            {
                Start = startDate,
                End = startDate + TimeSpan.FromDays(30),
                City = allCityNames[rand.Next(0, allCityNames.Count - 1)]
            });
        }

        return olympics;
    }

    private static DateTime RandomDay(Random rand, DateTime start, DateTime end)
    {
        var range = (end - start).Days;
        return start.AddDays(rand.Next(range));
    }
}