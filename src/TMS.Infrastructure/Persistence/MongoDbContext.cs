using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using TMS.Domain.Aggregates.Courses;
using TMS.Domain.Aggregates.Subjects;
using TMS.Domain.Aggregates.Teachers;

namespace TMS.Infrastructure.Persistence;

/// <summary>
/// Wraps a <see cref="IMongoDatabase"/> and exposes strongly-typed collection
/// accessors for every aggregate root and the domain-event log.
/// Registered as a singleton; <see cref="MongoDbSettings"/> is injected directly
/// so that the Infrastructure project does not need to take a dependency on
/// Microsoft.Extensions.Options (that wiring happens in DependencyInjection.cs).
/// </summary>
public sealed class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        _database = client.GetDatabase(settings.DatabaseName);
    }

    /// <summary>The <c>teachers</c> collection.</summary>
    public IMongoCollection<Teacher> Teachers =>
        _database.GetCollection<Teacher>("teachers");

    /// <summary>The <c>subjects</c> collection.</summary>
    public IMongoCollection<Subject> Subjects =>
        _database.GetCollection<Subject>("subjects");

    /// <summary>The <c>courses</c> collection.</summary>
    public IMongoCollection<Course> Courses =>
        _database.GetCollection<Course>("courses");

    /// <summary>The <c>domain_events</c> collection (append-only event log).</summary>
    public IMongoCollection<DomainEventDocument> DomainEvents =>
        _database.GetCollection<DomainEventDocument>("domain_events");
}

/// <summary>
/// Persistence document for a domain event stored in the <c>domain_events</c> collection.
/// </summary>
public sealed class DomainEventDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Discriminator string (e.g. "TeacherCreated").</summary>
    public string EventType { get; init; } = string.Empty;

    /// <summary>UTC timestamp when the event occurred inside the domain.</summary>
    public DateTime OccurredAt { get; init; }

    /// <summary>JSON-serialised payload of the original domain event.</summary>
    public string Payload { get; init; } = string.Empty;
}


/*MongoDbContext is the class that gives your application access to the MongoDB collections.
MongoDB is a document database, so instead of tables you generally work with collections.
DomainEventDocument is the MongoDB representation of a domain event.
This separates your domain model from your database model.*/