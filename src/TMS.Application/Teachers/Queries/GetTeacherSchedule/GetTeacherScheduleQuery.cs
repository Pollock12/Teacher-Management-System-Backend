using MediatR;
using TMS.Application.Teachers.DTOs;

namespace TMS.Application.Teachers.Queries.GetTeacherSchedule;

/// <summary>
/// Query to retrieve schedule entries for a specific teacher, optionally filtered by day.
/// Satisfies Requirements 8.7 and 8.8.
/// </summary>
public record GetTeacherScheduleQuery(Guid TeacherId, DayOfWeek? DayOfWeek = null)
    : IRequest<IReadOnlyList<ScheduleEntryDto>>;

/*
   Get a teacher's schedule, optionally for a specific day.
   Example,
     get teacher 123's entire schedule
     get teacher 123's MOnday schedule
*/
