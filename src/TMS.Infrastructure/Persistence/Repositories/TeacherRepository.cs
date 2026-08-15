using MongoDB.Bson;
using MongoDB.Driver;
using TMS.Domain.Aggregates.Teachers;
using TMS.Domain.Repositories;
using TMS.Infrastructure.Persistence;

namespace TMS.Infrastructure.Persistence.Repositories;

/// <summary>
/// MongoDB implementation of <see cref="ITeacherRepository"/>.
/// All queries exclude soft-deleted teachers (<c>IsDeleted == false</c>).
/// </summary>
public sealed class TeacherRepository : ITeacherRepository
{
    /*
    _context gives you access to the MongoDB teachers collection.
    Dependency Injection = giving a class the objects/services it needs from outside instead of making the class create those objects itself.
    TeacherRepository needs MongoDbContext, so give it to me when you create TeacherRepository.That's Dependency Injection.
    */
    private readonly MongoDbContext _context;

    public TeacherRepository(MongoDbContext context)
    {
        _context = context;
    }

    // ── GetByIdAsync ────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<Teacher?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var filter = Builders<Teacher>.Filter.And(
            Builders<Teacher>.Filter.Eq(t => t.IsDeleted, false),
            Builders<Teacher>.Filter.Eq(t => t.Id, id)
        );

        return await _context.Teachers
            .Find(filter)
            .FirstOrDefaultAsync(ct);
    }

    // ── GetByEmailAsync ─────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<Teacher?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var filter = Builders<Teacher>.Filter.And(
            Builders<Teacher>.Filter.Eq(t => t.IsDeleted, false),
            Builders<Teacher>.Filter.Eq(t => t.Email, email)
        );

        return await _context.Teachers
            .Find(filter)
            .FirstOrDefaultAsync(ct);
    }

    // ── GetPagedAsync ───────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<Teacher> Items, int TotalCount)> GetPagedAsync(
        string? firstName, string? lastName, string? email, Guid? subjectId,
        int pageNumber, int pageSize, CancellationToken ct = default)
    {
        // Start with the base "not deleted" filter
        var filters = new List<FilterDefinition<Teacher>>
        {
            Builders<Teacher>.Filter.Eq(t => t.IsDeleted, false)
        };

        // Optional case-insensitive regex filters on string fields
        // The "i" means case-insensitive
        if (firstName is not null)
            filters.Add(Builders<Teacher>.Filter.Regex("firstName", new BsonRegularExpression(firstName, "i")));

        if (lastName is not null)
            filters.Add(Builders<Teacher>.Filter.Regex("lastName", new BsonRegularExpression(lastName, "i")));

        if (email is not null)
            filters.Add(Builders<Teacher>.Filter.Regex("email", new BsonRegularExpression(email, "i")));

        // ElemMatch on the private backing field mapped as "subjectAssignments"
        // ElemMatch means find a teacher where at least one element inside subjectAssignments matches this condition.
        if (subjectId is not null)
        {
            filters.Add(
                Builders<Teacher>.Filter.ElemMatch<BsonDocument>(
                    "subjectAssignments",
                    Builders<BsonDocument>.Filter.Eq("subjectId", subjectId.ToString())
                )
            );
        }

        var compound = Builders<Teacher>.Filter.And(filters);

        // Run count and page query in parallel for efficiency
        var countTask = _context.Teachers.CountDocumentsAsync(compound, cancellationToken: ct);

        var itemsTask = _context.Teachers
            .Find(compound)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);

        await Task.WhenAll(countTask, itemsTask);

        var totalCount = (int)await countTask;
        var items = await itemsTask;

        return (items.AsReadOnly(), totalCount);
    }

    // ── GetAvailableAsync ───────────────────────────────────────────────────
    // Which teachers are available on a particular day and time?
    /// <inheritdoc/>
    public async Task<IReadOnlyList<Teacher>> GetAvailableAsync(
        DayOfWeek day, TimeOnly startTime, TimeOnly endTime, CancellationToken ct = default)
    {
        // Find teachers who have at least one availability slot that covers the
        // requested [startTime, endTime) range on the given day.
        // A slot covers the range when:
        //   slot.startTime < requested endTime   AND
        //   slot.endTime   > requested startTime
        var filter = Builders<Teacher>.Filter.And(
            Builders<Teacher>.Filter.Eq(t => t.IsDeleted, false),
            Builders<Teacher>.Filter.ElemMatch<BsonDocument>(
                "availabilitySlots",
                Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Eq("dayOfWeek", (int)day),
                    Builders<BsonDocument>.Filter.Lt("startTime", endTime.ToString("HH:mm:ss")),
                    Builders<BsonDocument>.Filter.Gt("endTime", startTime.ToString("HH:mm:ss"))
                )
            )
        );

        var results = await _context.Teachers
            .Find(filter)
            .ToListAsync(ct);

        return results.AsReadOnly();
    }

    // ── AddAsync ────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task AddAsync(Teacher teacher, CancellationToken ct = default)
    {
        await _context.Teachers.InsertOneAsync(teacher, cancellationToken: ct);
    }

    // ── UpdateAsync ─────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task UpdateAsync(Teacher teacher, CancellationToken ct = default)
    {
        var filter = Builders<Teacher>.Filter.Eq(t => t.Id, teacher.Id);
        await _context.Teachers.ReplaceOneAsync(filter, teacher, cancellationToken: ct);
    }
}

/*
    ITeacherRepository (I need these operations(Get, Add, Update) for Teachers. It doesn't say how the data will be stored.)
        ->
    TeacherRepository (promises to implement everything required by ITeacherRepository.)
        -> 
    MongoDbContext (the context is responsible for providing access to MongoDB)
        -> 
    _context.Teachers 
        -> 
    MongoDB "teachers" collection.

    ** ITeacherRepository defines the operations, TeacherRepository implements those operations,
    MongoDbContext provides MongoDB access and _context.Teachers represents the actual teachers collection in MongoDB.

*/
