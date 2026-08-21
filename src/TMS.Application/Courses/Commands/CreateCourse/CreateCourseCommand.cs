using MediatR;
using TMS.Application.Courses.DTOs;

namespace TMS.Application.Courses.Commands.CreateCourse;

/// <summary>
/// Command to create a new course linked to an existing subject.
/// Satisfies Requirements 8.4, 8.5.
/// </summary>
public record CreateCourseCommand(
    string Name,
    Guid SubjectId,
    string? Description
) : IRequest<CourseDto>;
