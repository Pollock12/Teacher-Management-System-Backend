using MediatR;
using TMS.Application.Subjects.DTOs;

namespace TMS.Application.Subjects.Queries.GetAllSubjects;

/// <summary>
/// Query to retrieve all active (non-deleted) subjects.
/// Satisfies Requirement 5.4.
/// </summary>
public record GetAllSubjectsQuery : IRequest<IReadOnlyList<SubjectDto>>;
