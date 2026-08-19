using MediatR;
using TMS.Application.Subjects.DTOs;
using TMS.Domain.Aggregates.Subjects;
using TMS.Domain.Exceptions;
using TMS.Domain.Repositories;

namespace TMS.Application.Subjects.Commands.CreateSubject;

/// <summary>
/// Handles <see cref="CreateSubjectCommand"/>.
/// Satisfies Requirements 5.1–5.3.
/// </summary>
public sealed class CreateSubjectCommandHandler : IRequestHandler<CreateSubjectCommand, SubjectDto>
{
    private readonly ISubjectRepository _subjectRepository;

    public CreateSubjectCommandHandler(ISubjectRepository subjectRepository)
    {
        _subjectRepository = subjectRepository;
    }

    public async Task<SubjectDto> Handle(
        CreateSubjectCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Check name uniqueness
        var existing = await _subjectRepository.GetByNameAsync(request.Name, cancellationToken);
        if (existing is not null)
            throw new ConflictException($"A subject with name '{request.Name}' already exists.");

        // 2. Create the Subject aggregate (no domain events raised per design)
        var subject = Subject.Create(request.Name, request.Description);

        // 3. Persist the subject
        await _subjectRepository.AddAsync(subject, cancellationToken);

        // 4. Return mapped DTO
        return new SubjectDto(
            subject.Id,
            subject.Name,
            subject.Description,
            subject.CreatedAt,
            subject.UpdatedAt);
    }
}
