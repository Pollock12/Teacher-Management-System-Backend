using MediatR;

namespace TMS.Application.Subjects.Commands.DeleteSubject;

/// <summary>
/// Command to soft-delete a subject by its ID.
/// Satisfies Requirements 5.5, 5.6.
/// </summary>
public record DeleteSubjectCommand(Guid SubjectId) : IRequest;
