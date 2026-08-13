namespace TMS.Domain.Aggregates.Teachers.Events;

/// <summary>
/// Raised when a Teacher is successfully assigned to a course via Teacher.AssignToCourse().
/// </summary>
public record TeacherAssignedToCourse(
    Guid TeacherId,
    Guid CourseId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime)
    : DomainEventBase
{
    public override string EventType => nameof(TeacherAssignedToCourse);
}
