namespace TMS.Infrastructure.Persistence;

/// <summary>
/// Configuration settings for connecting to MongoDB.
/// Bound from the "MongoDB" configuration section.
/// </summary>
public sealed class MongoDbSettings
{
    /// <summary>MongoDB connection string (e.g. "mongodb://localhost:27017").</summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>Name of the database to use.</summary>
    public string DatabaseName { get; set; } = string.Empty;
}

/* appsettings.json -> MongoDB -> MongoDbSettings -> MongoDbContext -> MongoClient -> MongoDB */