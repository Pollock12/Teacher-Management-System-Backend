using FluentValidation;

namespace TMS.Application.Subjects.Commands.CreateSubject;

/// <summary>
/// Validates <see cref="CreateSubjectCommand"/> before it reaches the handler.
/// Satisfies Requirements 5.2, 9.1.
/// </summary>
public sealed class CreateSubjectCommandValidator : AbstractValidator<CreateSubjectCommand>
{
    public CreateSubjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);
    }
}
