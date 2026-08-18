using MediatR;

namespace TMS.Application.Teachers.Commands.AssignTeacherToCourse;

/// <summary>
/// Command to assign a teacher to a course at a specific day and time slot.
/// Satisfies Requirements 8.1–8.5, 9.4, 10.5.
/// </summary>
public record AssignTeacherToCourseCommand(
    Guid TeacherId,
    Guid CourseId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime
) : IRequest;
