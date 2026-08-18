using MediatR;

namespace TMS.Application.Teachers.Commands.SetTeacherAvailability;

/// <summary>
/// Input record representing a single availability slot submitted by the client.
/// Mirrors <see cref="TMS.Domain.ValueObjects.AvailabilitySlot"/> but lives in the
/// Application layer so the API has no direct dependency on Domain value objects.
/// </summary>
public record AvailabilitySlotInput(
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime
);

/// <summary>
/// Command to replace a teacher's availability slots with the provided list.
/// Passing an empty list clears all existing availability.
/// Satisfies Requirements 7.1–7.4.
/// </summary>
public record SetTeacherAvailabilityCommand(
    Guid TeacherId,
    IReadOnlyList<AvailabilitySlotInput> Slots
) : IRequest;
