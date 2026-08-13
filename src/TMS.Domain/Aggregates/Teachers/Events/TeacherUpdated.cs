namespace TMS.Domain.Aggregates.Teachers.Events;

/// <summary>
/// Raised when a Teacher's profile fields are updated via Teacher.Update().
/// Null fields indicate that the corresponding field was not changed.
/// </summary>
public record TeacherUpdated(Guid TeacherId, string? FirstName, string? LastName, string? Email)
    : DomainEventBase
{
    public override string EventType => nameof(TeacherUpdated);
}
