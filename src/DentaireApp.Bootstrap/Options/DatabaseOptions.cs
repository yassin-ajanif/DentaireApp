namespace DentaireApp.Bootstrap.Options;

public enum DatabaseProvider
{
    Sqlite = 0,
    SqlServer = 1,
}

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";
    public DatabaseProvider Provider { get; set; } = DatabaseProvider.Sqlite;
    public string ConnectionString { get; set; } = "Data Source=dentaire.db";
}

