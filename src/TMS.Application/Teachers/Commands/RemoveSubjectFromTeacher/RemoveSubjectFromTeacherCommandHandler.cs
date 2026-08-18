using MediatR;
using TMS.Domain.Exceptions;
using TMS.Domain.Repositories;

namespace TMS.Application.Teachers.Commands.RemoveSubjectFromTeacher;

/// <summary>
/// Handles <see cref="RemoveSubjectFromTeacherCommand"/>.
/// Satisfies Requirement 6.5.
/// </summary>
public sealed class RemoveSubjectFromTeacherCommandHandler : IRequestHandler<RemoveSubjectFromTeacherCommand>
{
    private readonly ITeacherRepository _teacherRepository;

    public RemoveSubjectFromTeacherCommandHandler(ITeacherRepository teacherRepository)
    {
        _teacherRepository = teacherRepository;
    }

    public async Task Handle(RemoveSubjectFromTeacherCommand request, CancellationToken cancellationToken)
    {
        // 1. Load teacher by ID — throw 404 if not found (Req 6.5)
        var teacher = await _teacherRepository.GetByIdAsync(request.TeacherId, cancellationToken);
        if (teacher is null)
            throw new NotFoundException($"Teacher with ID '{request.TeacherId}' was not found.");

        // 2. Remove the subject from the teacher — domain method throws NotFoundException
        //    internally if the subject is not assigned to this teacher (Req 6.5)
        teacher.RemoveSubject(request.SubjectId);

        // 3. Persist the updated teacher aggregate (Req 6.5)
        //    Note: RemoveSubject does NOT emit domain events, so no IDomainEventRepository call needed.
        await _teacherRepository.UpdateAsync(teacher, cancellationToken);
    }
}
