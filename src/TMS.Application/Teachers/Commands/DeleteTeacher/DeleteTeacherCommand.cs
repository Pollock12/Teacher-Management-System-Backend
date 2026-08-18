using MediatR;

namespace TMS.Application.Teachers.Commands.DeleteTeacher;

/// <summary>
/// Command to soft-delete a teacher by ID.
/// Satisfies Requirements 3.1–3.4, 9.4, 10.3.
/// </summary>
public record DeleteTeacherCommand(Guid TeacherId) : IRequest;
