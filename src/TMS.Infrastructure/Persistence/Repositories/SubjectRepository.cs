using MongoDB.Bson;
using MongoDB.Driver;
using TMS.Domain.Aggregates.Subjects;
using TMS.Domain.Repositories;
using TMS.Infrastructure.Persistence;

namespace TMS.Infrastructure.Persistence.Repositories;

/// <summary>
/// MongoDB implementation of <see cref="ISubjectRepository"/>.
/// All queries exclude soft-deleted subjects (<c>IsDeleted == false</c>).
/// </summary>
public sealed class SubjectRepository : ISubjectRepository
{
    private readonly MongoDbContext _context;

    public SubjectRepository(MongoDbContext context)
    {
        _context = context;
    }

    // ── GetByIdAsync ────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<Subject?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var filter = Builders<Subject>.Filter.And(
            Builders<Subject>.Filter.Eq(s => s.IsDeleted, false),
            Builders<Subject>.Filter.Eq(s => s.Id, id)
        );

        return await _context.Subjects
            .Find(filter)
            .FirstOrDefaultAsync(ct);
    }

    // ── GetByNameAsync ──────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<Subject?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        var filter = Builders<Subject>.Filter.And(
            Builders<Subject>.Filter.Eq(s => s.IsDeleted, false),
            Builders<Subject>.Filter.Eq(s => s.Name, name)
        );

        return await _context.Subjects
            .Find(filter)
            .FirstOrDefaultAsync(ct);
    }

    // ── GetAllActiveAsync ───────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Subject>> GetAllActiveAsync(CancellationToken ct = default)
    {
        var filter = Builders<Subject>.Filter.Eq(s => s.IsDeleted, false);

        var results = await _context.Subjects
            .Find(filter)
            .ToListAsync(ct);

        return results.AsReadOnly();
    }

    // ── AddAsync ────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task AddAsync(Subject subject, CancellationToken ct = default)
    {
        await _context.Subjects.InsertOneAsync(subject, cancellationToken: ct);
    }

    // ── UpdateAsync ─────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task UpdateAsync(Subject subject, CancellationToken ct = default)
    {
        var filter = Builders<Subject>.Filter.Eq(s => s.Id, subject.Id);
        await _context.Subjects.ReplaceOneAsync(filter, subject, cancellationToken: ct);
    }

    // ── IsAssignedToAnyTeacherAsync ─────────────────────────────────────────

    /// <inheritdoc/>
    /// <remarks>
    /// Queries the <c>teachers</c> collection using ElemMatch on the
    /// <c>subjectAssignments</c> array to check whether at least one
    /// non-deleted teacher holds an assignment for the given subject.
    /// </remarks>
    
    /*
        Why does SubjectRepository look at Tecahers?
        We're inside SubjectRepository but we're querying Teachers.
        WHy?
        Because the question is : "Is this subject assigned to any teachers?"
        The assignnment information is stored inside the Teacher document.
    */
    public async Task<bool> IsAssignedToAnyTeacherAsync(Guid subjectId, CancellationToken ct = default)
    {
        // Look inside the teachers collection for any non-deleted teacher
        // that has at least one element in subjectAssignments whose subjectId
        // matches the given subjectId.
        // ElemMatch means : Find at least one element in this array that matches the condition.
        var filter = Builders<TMS.Domain.Aggregates.Teachers.Teacher>.Filter.And(
            Builders<TMS.Domain.Aggregates.Teachers.Teacher>.Filter.Eq(t => t.IsDeleted, false),
            Builders<TMS.Domain.Aggregates.Teachers.Teacher>.Filter.ElemMatch<BsonDocument>(
                "subjectAssignments",
                Builders<BsonDocument>.Filter.Eq("subjectId", subjectId.ToString())
            )
        );

        var count = await _context.Teachers.CountDocumentsAsync(filter, cancellationToken: ct);
        return count > 0;
    }
}
