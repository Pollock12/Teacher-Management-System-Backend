using MediatR;
using TMS.Domain.Exceptions;
using TMS.Domain.Repositories;

namespace TMS.Application.Teachers.Commands.AssignSubjectToTeacher;

/// <summary>
/// Handles <see cref="AssignSubjectToTeacherCommand"/>.
/// Satisfies Requirements 6.1–6.4, 9.2, 9.3, 10.4.
/// </summary>
public sealed class AssignSubjectToTeacherCommandHandler : IRequestHandler<AssignSubjectToTeacherCommand>
{
    private readonly ITeacherRepository _teacherRepository;
    private readonly ISubjectRepository _subjectRepository;
    private readonly IDomainEventRepository _domainEventRepository;

    public AssignSubjectToTeacherCommandHandler(
        ITeacherRepository teacherRepository,
        ISubjectRepository subjectRepository,
        IDomainEventRepository domainEventRepository)
    {
        _teacherRepository = teacherRepository;
        _subjectRepository = subjectRepository;
        _domainEventRepository = domainEventRepository;
    }

    public async Task Handle(AssignSubjectToTeacherCommand request, CancellationToken cancellationToken)
    {
        // 1. Load teacher by ID — throw 404 if not found (Req 6.2, 9.4)
        var teacher = await _teacherRepository.GetByIdAsync(request.TeacherId, cancellationToken);
        if (teacher is null)
            throw new NotFoundException($"Teacher with ID '{request.TeacherId}' was not found.");

        // 2. Load subject by ID — throw 404 if not found (Req 6.3)
        var subject = await _subjectRepository.GetByIdAsync(request.SubjectId, cancellationToken);
        if (subject is null)
            throw new NotFoundException($"Subject with ID '{request.SubjectId}' was not found.");

        // 3. Assign the subject to the teacher — throws ConflictException internally if duplicate (Req 6.4)
        teacher.AssignSubject(request.SubjectId);

        // 4. Persist domain events (SubjectAssignedToTeacher event) (Req 10.4)
        await _domainEventRepository.PersistAsync(teacher.DomainEvents, cancellationToken);

        // 5. Clear domain events from the aggregate
        teacher.ClearDomainEvents();

        // 6. Persist the updated teacher aggregate (Req 6.1)
        await _teacherRepository.UpdateAsync(teacher, cancellationToken);
    }
}
