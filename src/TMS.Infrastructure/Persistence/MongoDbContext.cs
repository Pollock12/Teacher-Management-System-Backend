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

    /// <summary>
    /// Creates all required indexes if they do not already exist.
    /// Safe to call on every application start (MongoDB is idempotent for index creation).
    /// </summary>
    public async Task EnsureIndexesAsync(CancellationToken ct = default)
    {
        // ── teachers.email — unique sparse ──────────────────────────────────
        var emailIndex = new CreateIndexModel<Teacher>(
            Builders<Teacher>.IndexKeys.Ascending(t => t.Email),
            new CreateIndexOptions { Unique = true, Sparse = true, Name = "idx_teachers_email_unique" });

        // ── teachers.isDeleted ──────────────────────────────────────────────
        var isDeletedIndex = new CreateIndexModel<Teacher>(
            Builders<Teacher>.IndexKeys.Ascending(t => t.IsDeleted),
            new CreateIndexOptions { Name = "idx_teachers_isDeleted" });

        // ── teachers.subjectAssignments.subjectId ───────────────────────────
        var subjectIdIndex = new CreateIndexModel<Teacher>(
            Builders<Teacher>.IndexKeys.Ascending("subjectAssignments.subjectId"),
            new CreateIndexOptions { Name = "idx_teachers_subjectAssignments_subjectId" });

        await Teachers.Indexes.CreateManyAsync(
            new[] { emailIndex, isDeletedIndex, subjectIdIndex }, ct);

        // ── domain_events.occurredAt ────────────────────────────────────────
        var occurredAtIndex = new CreateIndexModel<DomainEventDocument>(
            Builders<DomainEventDocument>.IndexKeys.Ascending(e => e.OccurredAt),
            new CreateIndexOptions { Name = "idx_domainevents_occurredAt" });

        // ── domain_events.eventType ─────────────────────────────────────────
        var eventTypeIndex = new CreateIndexModel<DomainEventDocument>(
            Builders<DomainEventDocument>.IndexKeys.Ascending(e => e.EventType),
            new CreateIndexOptions { Name = "idx_domainevents_eventType" });

        await DomainEvents.Indexes.CreateManyAsync(
            new[] { occurredAtIndex, eventTypeIndex }, ct);
    }
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