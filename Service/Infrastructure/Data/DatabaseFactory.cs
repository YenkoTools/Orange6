using Infrastructure.Configuration;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using System.Data;

namespace Infrastructure.Data;

/// <summary>
/// Factory for creating SQLite database connections.
/// </summary>
public class DatabaseFactory : IDatabaseFactory
{
    private readonly DatabaseOptions _options;

    public DatabaseFactory(IOptions<DatabaseOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// Creates and returns a new SQLite database connection.
    /// </summary>
    public IDbConnection CreateConnection()
    {
        return new SqliteConnection($"Data Source={_options.DatabasePath}.sqlite");
    }
}
