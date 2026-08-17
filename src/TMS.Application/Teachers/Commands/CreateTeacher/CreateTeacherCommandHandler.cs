using MediatR;
using TMS.Application.Teachers.DTOs;
using TMS.Domain.Aggregates.Teachers;
using TMS.Domain.Exceptions;
using TMS.Domain.Repositories;

namespace TMS.Application.Teachers.Commands.CreateTeacher;

/// <summary>
/// Handles <see cref="CreateTeacherCommand"/>.
/// Satisfies Requirements 1.1–1.6, 10.1.
/// </summary>
public sealed class CreateTeacherCommandHandler : IRequestHandler<CreateTeacherCommand, TeacherDto>
{
    private readonly ITeacherRepository _teacherRepository; // Used to save/find the teacher
    private readonly IDomainEventRepository _domainEventRepository; // Used to save domain events

    public CreateTeacherCommandHandler(
        ITeacherRepository teacherRepository,
        IDomainEventRepository domainEventRepository)
    {
        _teacherRepository = teacherRepository;
        _domainEventRepository = domainEventRepository;
    }

    public async Task<TeacherDto> Handle(
        CreateTeacherCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Check email uniqueness
        var existing = await _teacherRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
            throw new ConflictException($"A teacher with email '{request.Email}' already exists.");

        // 2. Create the Teacher aggregate (raises TeacherCreated domain event)
        var teacher = Teacher.Create(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            request.DateOfBirth,
            request.Address);

        // 3. Persist domain events
        // This saves the event into MongoDB.
        await _domainEventRepository.PersistAsync(teacher.DomainEvents, cancellationToken);

        // 4. Persist the teacher
        // This saves the actual Teacher
        await _teacherRepository.AddAsync(teacher, cancellationToken);

        // 5. Return mapped DTO
        // Convert Teacher to DTO
        return teacher.ToDto();
    }
}

//Teacher.Create() -> Teacher created + TeacherCreated event
