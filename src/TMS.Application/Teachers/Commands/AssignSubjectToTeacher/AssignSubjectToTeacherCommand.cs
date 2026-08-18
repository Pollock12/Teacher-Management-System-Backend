using MediatR;

namespace TMS.Application.Teachers.Commands.AssignSubjectToTeacher;

/// <summary>
/// Command to assign a subject to an existing teacher.
/// Satisfies Requirements 6.1–6.4, 9.2, 9.3, 10.4.
/// </summary>
public record AssignSubjectToTeacherCommand(
    Guid TeacherId,
    Guid SubjectId
) : IRequest;


// IRequest means there is no return value.