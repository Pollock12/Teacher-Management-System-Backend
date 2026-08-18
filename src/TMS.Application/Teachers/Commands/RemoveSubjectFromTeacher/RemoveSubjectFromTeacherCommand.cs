using MediatR;

namespace TMS.Application.Teachers.Commands.RemoveSubjectFromTeacher;

/// <summary>
/// Command to remove a subject assignment from an existing teacher.
/// Satisfies Requirement 6.5.
/// </summary>
public record RemoveSubjectFromTeacherCommand(
    Guid TeacherId,
    Guid SubjectId
) : IRequest;
