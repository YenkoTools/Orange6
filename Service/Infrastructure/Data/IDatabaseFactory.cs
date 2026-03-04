using System.Data;

namespace Infrastructure.Data;

/// <summary>
/// Defines the contract for creating database connections.
/// </summary>
public interface IDatabaseFactory
{
    IDbConnection CreateConnection();
}
