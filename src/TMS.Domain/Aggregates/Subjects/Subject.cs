using TMS.Domain.Common;

namespace TMS.Domain.Aggregates.Subjects;

/// <summary>
/// Subject aggregate root. Represents a teachable subject that can be assigned to teachers.
/// </summary>
public sealed class Subject : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsDeleted { get; private set; }

    // Private constructor — only the factory method can create instances
    private Subject() { }

    // ── Factory Method ──────────────────────────────────────────────────────
    /// <summary>
    /// Creates a new Subject aggregate.
    /// </summary>
    /// <param name="name">Non-empty name, at most 200 characters.</param>
    /// <param name="description">Optional description.</param>
    /// <returns>A valid <see cref="Subject"/> with a new Id. No domain event is raised.</returns>
    public static Subject Create(string name, string? description = null)
    {
        return new Subject
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    // ── Soft Delete ─────────────────────────────────────────────────────────
    /// <summary>
    /// Marks the subject as deleted.
    /// </summary>
    /// <remarks>
    /// Precondition: subject must not be currently assigned to any teacher.
    /// That constraint is enforced in the application layer handler.
    /// </remarks>
    public void SoftDelete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
