namespace Infrastructure.Configuration;

/// <summary>
/// Configuration options for the SQLite database.
/// </summary>
public class DatabaseOptions
{
    public const string SectionName = "Database";

    /// <summary>
    /// Gets or sets the path (including file name without extension) of the SQLite database.
    /// </summary>
    public string DatabasePath { get; set; } = "orange6-db";
}
