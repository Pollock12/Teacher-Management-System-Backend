using MediatR;
using TMS.Domain.Exceptions;
using TMS.Domain.Repositories;
using TMS.Domain.ValueObjects;

namespace TMS.Application.Teachers.Commands.SetTeacherAvailability;

/// <summary>
/// Handles <see cref="SetTeacherAvailabilityCommand"/>.
/// Replaces all existing availability slots on the teacher with the provided list.
/// Satisfies Requirements 7.1–7.4.
/// </summary>
public sealed class SetTeacherAvailabilityCommandHandler
    : IRequestHandler<SetTeacherAvailabilityCommand>
{
    private readonly ITeacherRepository _teacherRepository;

    public SetTeacherAvailabilityCommandHandler(ITeacherRepository teacherRepository)
    {
        _teacherRepository = teacherRepository;
    }

    public async Task Handle(
        SetTeacherAvailabilityCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Load the teacher — throw 404 if not found (Req 7.4, 9.2)
        var teacher = await _teacherRepository.GetByIdAsync(request.TeacherId, cancellationToken);
        if (teacher is null)
            throw new NotFoundException($"Teacher with ID '{request.TeacherId}' was not found.");

        // 2. Map AvailabilitySlotInput → AvailabilitySlot domain value objects
        //    AvailabilitySlot constructor enforces StartTime < EndTime (already validated above,
        //    but the domain invariant provides a second guard).
        var slots = request.Slots
            .Select(s => new AvailabilitySlot(s.DayOfWeek, s.StartTime, s.EndTime))
            .ToList();

        // 3. Replace the teacher's availability (Req 7.1)
        teacher.SetAvailability(slots);

        // 4. Persist the updated teacher aggregate (Req 7.1)
        await _teacherRepository.UpdateAsync(teacher, cancellationToken);
    }
}
