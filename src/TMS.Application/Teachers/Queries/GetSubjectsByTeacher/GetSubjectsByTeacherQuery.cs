using MediatR;
using TMS.Application.Subjects.DTOs;

namespace TMS.Application.Teachers.Queries.GetSubjectsByTeacher;

/// <summary>
/// Query to retrieve all subjects currently assigned to a specific teacher.
/// Satisfies Requirement 6.6.
/// </summary>
public record GetSubjectsByTeacherQuery(Guid TeacherId) : IRequest<IReadOnlyList<SubjectDto>>;

// This code is a Query Handler that gets all subjects assigned to a particular teacher.