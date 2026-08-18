using FluentValidation;

namespace TMS.Application.Teachers.Commands.AssignTeacherToCourse;

/// <summary>
/// Validates <see cref="AssignTeacherToCourseCommand"/> before it reaches the handler.
/// Satisfies Requirements 9.4.
/// </summary>
public sealed class AssignTeacherToCourseCommandValidator : AbstractValidator<AssignTeacherToCourseCommand>
{
    public AssignTeacherToCourseCommandValidator()
    {
        RuleFor(x => x.TeacherId)
            .NotEmpty();

        RuleFor(x => x.CourseId)
            .NotEmpty();

        RuleFor(x => x.StartTime)
            .Must((command, startTime) => startTime < command.EndTime)
            .WithMessage("StartTime must be earlier than EndTime.");
    }
}
