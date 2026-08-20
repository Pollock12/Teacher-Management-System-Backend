using MediatR;
using TMS.Application.Teachers.DTOs;
using TMS.Domain.Repositories;

namespace TMS.Application.Teachers.Queries.GetAvailableTeachers;

/// <summary>
/// Handles <see cref="GetAvailableTeachersQuery"/>.
/// Delegates to <see cref="ITeacherRepository.GetAvailableAsync"/> to find all teachers
/// whose availability slots include the requested day and overlap the given time range,
/// then maps each result to a <see cref="TeacherSummaryDto"/>.
/// Satisfies Requirement 7.6.
/// </summary>
public sealed class GetAvailableTeachersQueryHandler
    : IRequestHandler<GetAvailableTeachersQuery, IReadOnlyList<TeacherSummaryDto>>
{
    private readonly ITeacherRepository _teacherRepository;

    public GetAvailableTeachersQueryHandler(ITeacherRepository teacherRepository)
    {
        _teacherRepository = teacherRepository;
    }

    public async Task<IReadOnlyList<TeacherSummaryDto>> Handle(
        GetAvailableTeachersQuery query,
        CancellationToken cancellationToken)
    {
        var teachers = await _teacherRepository.GetAvailableAsync(
            query.DayOfWeek,
            query.StartTime,
            query.EndTime,
            cancellationToken);

        return teachers
            .Select(t => t.ToSummaryDto())
            .ToList();
    }
}
