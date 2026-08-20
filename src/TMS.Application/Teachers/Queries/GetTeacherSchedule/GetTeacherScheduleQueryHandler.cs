using MediatR;
using TMS.Application.Teachers.DTOs;
using TMS.Domain.Exceptions;
using TMS.Domain.Repositories;

namespace TMS.Application.Teachers.Queries.GetTeacherSchedule;

/// <summary>
/// Handles <see cref="GetTeacherScheduleQuery"/>.
/// Loads the teacher from the repository, throws <see cref="NotFoundException"/> when not found,
/// and maps the teacher's schedule entries to a list of <see cref="ScheduleEntryDto"/>.
/// When <see cref="GetTeacherScheduleQuery.DayOfWeek"/> is provided, only entries matching
/// that day are returned. Satisfies Requirements 8.7 and 8.8.
/// </summary>
public sealed class GetTeacherScheduleQueryHandler
    : IRequestHandler<GetTeacherScheduleQuery, IReadOnlyList<ScheduleEntryDto>>
{
    private readonly ITeacherRepository _teacherRepository;

    public GetTeacherScheduleQueryHandler(ITeacherRepository teacherRepository)
    {
        _teacherRepository = teacherRepository;
    }

    public async Task<IReadOnlyList<ScheduleEntryDto>> Handle(
        GetTeacherScheduleQuery query,
        CancellationToken cancellationToken)
    {
        var teacher = await _teacherRepository.GetByIdAsync(query.TeacherId, cancellationToken);

        if (teacher is null)
            throw new NotFoundException($"Teacher with ID '{query.TeacherId}' was not found.");

        var entries = query.DayOfWeek.HasValue
            ? teacher.ScheduleEntries.Where(e => e.DayOfWeek == query.DayOfWeek.Value)
            : teacher.ScheduleEntries;

        return entries
            .Select(e => new ScheduleEntryDto(e.CourseId, e.DayOfWeek, e.StartTime, e.EndTime))
            .ToList();
    }
}
