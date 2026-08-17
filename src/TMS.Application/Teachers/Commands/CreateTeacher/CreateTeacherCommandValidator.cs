using FluentValidation;

namespace TMS.Application.Teachers.Commands.CreateTeacher;

/// <summary>
/// Validates <see cref="CreateTeacherCommand"/> before it reaches the handler.
/// Satisfies Requirements 9.1, 9.3.
/// </summary>
public sealed class CreateTeacherCommandValidator : AbstractValidator<CreateTeacherCommand>
{
    public CreateTeacherCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);
    }
}
