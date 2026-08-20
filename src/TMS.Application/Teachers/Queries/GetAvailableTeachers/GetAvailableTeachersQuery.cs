using MediatR;
using TMS.Application.Teachers.DTOs;

namespace TMS.Application.Teachers.Queries.GetAvailableTeachers;

/// <summary>
/// Query to retrieve all teachers whose availability covers the specified
/// day of week and overlaps the given time range.
/// Satisfies Requirement 7.6.
/// </summary>
public record GetAvailableTeachersQuery(
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime
) : IRequest<IReadOnlyList<TeacherSummaryDto>>;

// Which teachers are available at this time?
