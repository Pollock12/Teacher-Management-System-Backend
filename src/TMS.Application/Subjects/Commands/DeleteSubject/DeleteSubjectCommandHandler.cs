using MediatR;
using TMS.Domain.Exceptions;
using TMS.Domain.Repositories;

namespace TMS.Application.Subjects.Commands.DeleteSubject;

/// <summary>
/// Handles <see cref="DeleteSubjectCommand"/>.
/// Satisfies Requirements 5.5, 5.6.
/// </summary>
public sealed class DeleteSubjectCommandHandler : IRequestHandler<DeleteSubjectCommand>
{
    private readonly ISubjectRepository _subjectRepository;

    public DeleteSubjectCommandHandler(ISubjectRepository subjectRepository)
    {
        _subjectRepository = subjectRepository;
    }

    public async Task Handle(DeleteSubjectCommand request, CancellationToken cancellationToken)
    {
        // 1. Load subject by ID — throw 404 if not found (Req 5.5)
        var subject = await _subjectRepository.GetByIdAsync(request.SubjectId, cancellationToken);
        if (subject is null)
            throw new NotFoundException($"Subject with ID '{request.SubjectId}' was not found.");

        // 2. Guard against deletion when subject is assigned to any teacher (Req 5.6)
        var isAssigned = await _subjectRepository.IsAssignedToAnyTeacherAsync(request.SubjectId, cancellationToken);
        if (isAssigned)
            throw new DomainRuleException(
                $"Subject with ID '{request.SubjectId}' cannot be deleted because it is currently assigned to one or more teachers.");

        // 3. Soft-delete the subject
        subject.SoftDelete();

        // 4. Persist the updated subject aggregate
        await _subjectRepository.UpdateAsync(subject, cancellationToken);
    }
}
