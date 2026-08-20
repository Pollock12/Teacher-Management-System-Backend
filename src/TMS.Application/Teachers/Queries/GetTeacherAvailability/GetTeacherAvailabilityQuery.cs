using MediatR;
using TMS.Application.Teachers.DTOs;

namespace TMS.Application.Teachers.Queries.GetTeacherAvailability;

/// <summary>
/// Query to retrieve all availability slots for a specific teacher.
/// Satisfies Requirement 7.5.
/// </summary>
public record GetTeacherAvailabilityQuery(Guid TeacherId) : IRequest<IReadOnlyList<AvailabilitySlotDto>>;
