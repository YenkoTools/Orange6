using Dapper;
using Microsoft.Extensions.Logging;

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
    /// Creates the database schema if it does not already exist.
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

        _logger.LogInformation("Database initialized.");
    }
}
