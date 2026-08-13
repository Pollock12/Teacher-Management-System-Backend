namespace TMS.Domain.Aggregates.Teachers.Events;

/// <summary>
/// Raised when a new Teacher is successfully created via Teacher.Create().
/// </summary>
public record TeacherCreated(Guid TeacherId, string FirstName, string LastName, string Email)
    : DomainEventBase
{
    public override string EventType => nameof(TeacherCreated);
}
