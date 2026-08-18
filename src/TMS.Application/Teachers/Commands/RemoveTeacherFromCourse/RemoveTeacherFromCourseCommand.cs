using MediatR;

namespace TMS.Application.Teachers.Commands.RemoveTeacherFromCourse;

/// <summary>
/// Command to remove a teacher from a course assignment.
/// Satisfies Requirement 8.6.
/// </summary>
public record RemoveTeacherFromCourseCommand(
    Guid TeacherId,
    Guid CourseId
) : IRequest;
