using TMS.Domain.Common;

namespace TMS.Domain.Aggregates.Courses;

/// <summary>
/// Course aggregate root. Represents a course linked to a subject.
/// </summary>
public sealed class Course : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid SubjectId { get; private set; }
    public bool IsDeleted { get; private set; }

    // Private constructor — only the factory method can create instances
    private Course() { }

    // ── Factory Method ──────────────────────────────────────────────────────
    /// <summary>
    /// Creates a new Course aggregate.
    /// </summary>
    /// <param name="name">Non-empty course name.</param>
    /// <param name="subjectId">Valid, non-empty subject identifier.</param>
    /// <param name="description">Optional description.</param>
    /// <returns>A valid <see cref="Course"/> with a new Id.</returns>
    public static Course Create(string name, Guid subjectId, string? description = null)
    {
        return new Course
        {
            Id = Guid.NewGuid(),
            Name = name,
            SubjectId = subjectId,
            Description = description,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
