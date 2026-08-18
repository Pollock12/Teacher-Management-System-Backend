using FluentValidation;

namespace TMS.Application.Teachers.Commands.AssignSubjectToTeacher;

/// <summary>
/// Validates <see cref="AssignSubjectToTeacherCommand"/> before it reaches the handler.
/// Satisfies Requirements 9.2, 9.3.
/// </summary>
public sealed class AssignSubjectToTeacherCommandValidator : AbstractValidator<AssignSubjectToTeacherCommand>
{
    public AssignSubjectToTeacherCommandValidator()
    {
        RuleFor(x => x.TeacherId)
            .NotEmpty();

        RuleFor(x => x.SubjectId)
            .NotEmpty();
    }
}
