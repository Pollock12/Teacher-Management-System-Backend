using MongoDB.Driver;
using TMS.Domain.Aggregates.Courses;
using TMS.Domain.Repositories;
using TMS.Infrastructure.Persistence;

namespace TMS.Infrastructure.Persistence.Repositories;

/// <summary>
/// MongoDB implementation of <see cref="ICourseRepository"/>.
/// All queries exclude soft-deleted courses (<c>IsDeleted == false</c>).
/// </summary>
public sealed class CourseRepository : ICourseRepository
{
    private readonly MongoDbContext _context;

    public CourseRepository(MongoDbContext context)
    {
        _context = context;
    }

    // ── GetByIdAsync ────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<Course?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var filter = Builders<Course>.Filter.And(
            Builders<Course>.Filter.Eq(c => c.IsDeleted, false),
            Builders<Course>.Filter.Eq(c => c.Id, id)
        );

        return await _context.Courses
            .Find(filter)
            .FirstOrDefaultAsync(ct);
    }

    // ── AddAsync ────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task AddAsync(Course course, CancellationToken ct = default)
    {
        await _context.Courses.InsertOneAsync(course, cancellationToken: ct);
    }
}
