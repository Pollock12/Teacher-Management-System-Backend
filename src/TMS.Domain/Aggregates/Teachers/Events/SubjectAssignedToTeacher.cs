namespace TMS.Domain.Aggregates.Teachers.Events;

/// <summary>
/// Raised when a subject is successfully assigned to a Teacher via Teacher.AssignSubject().
/// </summary>
public record SubjectAssignedToTeacher(Guid TeacherId, Guid SubjectId)
    : DomainEventBase
{
    public override string EventType => nameof(SubjectAssignedToTeacher);
}
