using MediatR;
using TMS.Application.Teachers.DTOs;
using TMS.Domain.Exceptions;
using TMS.Domain.Repositories;

namespace TMS.Application.Teachers.Queries.GetTeacherAvailability;

/// <summary>
/// Handles <see cref="GetTeacherAvailabilityQuery"/>.
/// Loads the teacher from the repository, throws <see cref="NotFoundException"/> when not found,
/// and maps the teacher's availability slots to a list of <see cref="AvailabilitySlotDto"/>.
/// Satisfies Requirement 7.5.
/// </summary>
public sealed class GetTeacherAvailabilityQueryHandler
    : IRequestHandler<GetTeacherAvailabilityQuery, IReadOnlyList<AvailabilitySlotDto>>
{
    private readonly ITeacherRepository _teacherRepository;

    public GetTeacherAvailabilityQueryHandler(ITeacherRepository teacherRepository)
    {
        _teacherRepository = teacherRepository;
    }

    public async Task<IReadOnlyList<AvailabilitySlotDto>> Handle(
        GetTeacherAvailabilityQuery query,
        CancellationToken cancellationToken)
    {
        var teacher = await _teacherRepository.GetByIdAsync(query.TeacherId, cancellationToken);

        if (teacher is null)
            throw new NotFoundException($"Teacher with ID '{query.TeacherId}' was not found.");

        return teacher.AvailabilitySlots
            .Select(s => new AvailabilitySlotDto(s.DayOfWeek, s.StartTime, s.EndTime))
            .ToList();
    }
}
