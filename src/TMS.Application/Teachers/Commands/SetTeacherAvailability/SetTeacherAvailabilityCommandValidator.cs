using FluentValidation;

namespace TMS.Application.Teachers.Commands.SetTeacherAvailability;

/// <summary>
/// Validates <see cref="SetTeacherAvailabilityCommand"/> before it reaches the handler.
/// Satisfies Requirements 7.2, 7.3, 9.1.
/// </summary>
public sealed class SetTeacherAvailabilityCommandValidator
    : AbstractValidator<SetTeacherAvailabilityCommand>
{
    public SetTeacherAvailabilityCommandValidator()
    {
        // Req 7.4: teacher must be identified
        RuleFor(x => x.TeacherId)
            .NotEmpty()
            .WithMessage("TeacherId must not be empty.");

        // Req 7.2 / 7.3: each slot must have a valid day and a start time strictly before end time
        RuleForEach(x => x.Slots).ChildRules(slot =>
        {
            slot.RuleFor(s => s.StartTime)
                .LessThan(s => s.EndTime)
                .WithMessage("StartTime must be earlier than EndTime.");
        });
    }
}
