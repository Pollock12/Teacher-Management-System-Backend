namespace TMS.Domain.Aggregates.Teachers.Events;

/// <summary>
/// Raised when a Teacher is soft-deleted via Teacher.SoftDelete().
/// </summary>
public record TeacherDeleted(Guid TeacherId)
    : DomainEventBase
{
    public override string EventType => nameof(TeacherDeleted);
}
