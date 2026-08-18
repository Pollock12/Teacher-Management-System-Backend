using MediatR;
using TMS.Domain.Exceptions;
using TMS.Domain.Repositories;

namespace TMS.Application.Teachers.Commands.DeleteTeacher;

/// <summary>
/// Handles <see cref="DeleteTeacherCommand"/>.
/// Satisfies Requirements 3.1–3.4, 9.4, 10.3.
/// </summary>
public sealed class DeleteTeacherCommandHandler : IRequestHandler<DeleteTeacherCommand>
{
    private readonly ITeacherRepository _teacherRepository;
    private readonly IDomainEventRepository _domainEventRepository;

    public DeleteTeacherCommandHandler(
        ITeacherRepository teacherRepository,
        IDomainEventRepository domainEventRepository)
    {
        _teacherRepository = teacherRepository;
        _domainEventRepository = domainEventRepository;
    }

    public async Task Handle(DeleteTeacherCommand request, CancellationToken cancellationToken)
    {
        // 1. Load teacher by ID — throw 404 if not found (Req 3.2, 9.4)
        var teacher = await _teacherRepository.GetByIdAsync(request.TeacherId, cancellationToken);
        if (teacher is null)
            throw new NotFoundException($"Teacher with ID '{request.TeacherId}' was not found.");

        // 2. Soft-delete — throws DomainRuleException if teacher has active schedule entries (Req 3.4)
        teacher.SoftDelete();

        // 3. Persist domain events (TeacherDeleted event) (Req 10.3)
        await _domainEventRepository.PersistAsync(teacher.DomainEvents, cancellationToken);

        // 4. Clear domain events from the aggregate
        teacher.ClearDomainEvents();

        // 5. Persist the updated teacher aggregate (Req 3.1, 3.3)
        await _teacherRepository.UpdateAsync(teacher, cancellationToken);
    }
}
