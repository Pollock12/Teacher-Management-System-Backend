using MediatR;
using TMS.Application.Teachers.DTOs;

namespace TMS.Application.Teachers.Queries.GetTeacherById;

/// <summary>
/// Query to retrieve a single teacher by their unique identifier.
/// Satisfies Requirements 4.1, 4.2.
/// </summary>
public record GetTeacherByIdQuery(Guid TeacherId) : IRequest<TeacherDto>;
