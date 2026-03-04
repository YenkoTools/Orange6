using Dapper;
using Microsoft.Extensions.Logging;
using System.Data;

namespace Infrastructure.Data;

/// <summary>
/// Initializes the SQLite database schema when the application starts.
/// </summary>
public class DatabaseInitializer
{
    private readonly IDatabaseFactory _databaseFactory;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(IDatabaseFactory databaseFactory, ILogger<DatabaseInitializer> logger)
    {
        _databaseFactory = databaseFactory;
        _logger = logger;
    }

    /// <summary>
    /// Creates the database schema if it does not already exist and seeds sample data.
    /// </summary>
    public void Initialize()
    {
        using var connection = _databaseFactory.CreateConnection();
        connection.Open();

        connection.Execute("""
            CREATE TABLE IF NOT EXISTS Users (
                Id        INTEGER PRIMARY KEY AUTOINCREMENT,
                Username  TEXT    NOT NULL,
                Email     TEXT    NOT NULL,
                FirstName TEXT    NOT NULL,
                LastName  TEXT    NOT NULL,
                CreatedAt TEXT    NOT NULL,
                UpdatedAt TEXT    NOT NULL
            )
        """);

        SeedSampleData(connection);

        _logger.LogInformation("Database initialized.");
    }

    private static void SeedSampleData(IDbConnection connection)
    {
        var count = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Users");
        if (count > 0) return;

        var now = DateTime.UtcNow.ToString("o");
        foreach (var user in GetSampleUsers())
        {
            connection.Execute("""
                INSERT INTO Users (Username, Email, FirstName, LastName, CreatedAt, UpdatedAt)
                VALUES (@Username, @Email, @FirstName, @LastName, @CreatedAt, @UpdatedAt)
                """,
                new { user.Username, user.Email, user.FirstName, user.LastName, CreatedAt = now, UpdatedAt = now });
        }
    }

    /// <summary>
    /// Returns sample Harry Potter characters to seed the database.
    /// </summary>
    private static IEnumerable<(string Username, string Email, string FirstName, string LastName)> GetSampleUsers() =>
    [
        ("harrypotter",       "harry.potter@hogwarts.edu",         "Harry",      "Potter"),
        ("hermionegranger",   "hermione.granger@hogwarts.edu",     "Hermione",   "Granger"),
        ("ronweasley",        "ron.weasley@hogwarts.edu",          "Ron",        "Weasley"),
        ("albusdumbledore",   "albus.dumbledore@hogwarts.edu",     "Albus",      "Dumbledore"),
        ("severussnape",      "severus.snape@hogwarts.edu",        "Severus",    "Snape"),
        ("lordvoldemort",     "lord.voldemort@darkarts.com",       "Lord",       "Voldemort"),
        ("dracomalfoy",       "draco.malfoy@hogwarts.edu",         "Draco",      "Malfoy"),
        ("ginnyweasley",      "ginny.weasley@hogwarts.edu",        "Ginny",      "Weasley"),
        ("nevillelongbottom", "neville.longbottom@hogwarts.edu",   "Neville",    "Longbottom"),
        ("lunalovegood",      "luna.lovegood@hogwarts.edu",        "Luna",       "Lovegood"),
        ("siriusblack",       "sirius.black@wizardingworld.com",   "Sirius",     "Black"),
        ("rubeshagrid",       "rubeus.hagrid@hogwarts.edu",        "Rubeus",     "Hagrid"),
        ("minervamcgonagall", "minerva.mcgonagall@hogwarts.edu",   "Minerva",    "McGonagall"),
        ("remuslupin",        "remus.lupin@hogwarts.edu",          "Remus",      "Lupin"),
        ("fredweasley",       "fred.weasley@hogwarts.edu",         "Fred",       "Weasley"),
        ("georgeweasley",     "george.weasley@hogwarts.edu",       "George",     "Weasley"),
        ("percyweasley",      "percy.weasley@hogwarts.edu",        "Percy",      "Weasley"),
        ("arthurweasley",     "arthur.weasley@hogwarts.edu",       "Arthur",     "Weasley"),
        ("mollyweasley",      "molly.weasley@hogwarts.edu",        "Molly",      "Weasley"),
        ("bellatrixlestrange","bellatrix.lestrange@darkarts.com",  "Bellatrix",  "Lestrange"),
        ("nymphadotonks",     "nymphadora.tonks@wizardingworld.com","Nymphadora", "Tonks"),
        ("cedricdiggory",     "cedric.diggory@hogwarts.edu",       "Cedric",     "Diggory"),
        ("chochang",          "cho.chang@hogwarts.edu",            "Cho",        "Chang"),
        ("lavenderbrown",     "lavender.brown@hogwarts.edu",       "Lavender",   "Brown"),
        ("dobby",             "dobby@hogwarts.edu",                "Dobby",      "HouseElf"),
    ];
}
