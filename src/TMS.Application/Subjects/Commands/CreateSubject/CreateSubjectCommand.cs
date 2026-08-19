using MediatR;
using TMS.Application.Subjects.DTOs;

namespace TMS.Application.Subjects.Commands.CreateSubject;

/// <summary>
/// Command to create a new subject.
/// Satisfies Requirements 5.1–5.3.
/// </summary>
public record CreateSubjectCommand(
    string Name,
    string? Description
) : IRequest<SubjectDto>;
