using FluentValidation;

namespace TMS.Application.Teachers.Commands.UpdateTeacher;

/// <summary>
/// Validates <see cref="UpdateTeacherCommand"/> before it reaches the handler.
/// Satisfies Requirements 2.2, 9.2, 9.3.
/// </summary>
public sealed class UpdateTeacherCommandValidator : AbstractValidator<UpdateTeacherCommand>
{
    public UpdateTeacherCommandValidator()
    {
        RuleFor(x => x.TeacherId)
            .NotEmpty()
            .WithMessage("TeacherId must not be empty.");

        // At least one updatable field must be provided (Req 2.2)
        RuleFor(x => x)
            .Must(x =>
                x.FirstName is not null ||
                x.LastName is not null ||
                x.Email is not null ||
                x.PhoneNumber is not null ||
                x.DateOfBirth is not null ||
                x.Address is not null)
            .WithMessage("At least one field must be provided for update.");

        // Optional field constraints — only validated when a value is supplied
        When(x => x.FirstName is not null, () =>
        {
            RuleFor(x => x.FirstName)
                .MaximumLength(100)
                .WithMessage("First name must not exceed 100 characters.");
        });

        When(x => x.LastName is not null, () =>
        {
            RuleFor(x => x.LastName)
                .MaximumLength(100)
                .WithMessage("Last name must not exceed 100 characters.");
        });

        When(x => x.Email is not null, () =>
        {
            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage("Email must be a valid email address.")
                .MaximumLength(255)
                .WithMessage("Email must not exceed 255 characters.");
        });
    }
}
