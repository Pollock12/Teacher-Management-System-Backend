using FluentValidation;

namespace TMS.Application.Courses.Commands.CreateCourse;

public sealed class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.SubjectId)
            .NotEmpty()
            .WithMessage("SubjectId must not be an empty Guid.");
    }
}
