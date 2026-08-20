using MediatR;
using TMS.Application.Common;
using TMS.Application.Teachers.DTOs;

namespace TMS.Application.Teachers.Queries.GetAllTeachers;

/// <summary>
/// Query to retrieve a paginated list of all active teachers, with optional filters.
/// Satisfies Requirements 4.3, 4.4, 4.5, 4.6.
/// </summary>
public record GetAllTeachersQuery(
    string? FirstName = null,
    string? LastName = null,
    string? Email = null,
    Guid? SubjectId = null,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<PagedResult<TeacherSummaryDto>>;
