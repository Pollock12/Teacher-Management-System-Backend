using MediatR;
using TMS.Application.Teachers.DTOs;
using TMS.Domain.Exceptions;
using TMS.Domain.Repositories;

namespace TMS.Application.Teachers.Commands.UpdateTeacher;

/// <summary>
/// Handles <see cref="UpdateTeacherCommand"/>.
/// Satisfies Requirements 2.1–2.5, 9.2, 9.3, 10.2.
/// </summary>
public sealed class UpdateTeacherCommandHandler : IRequestHandler<UpdateTeacherCommand, TeacherDto>
{
    private readonly ITeacherRepository _teacherRepository;
    private readonly IDomainEventRepository _domainEventRepository;

    public UpdateTeacherCommandHandler(
        ITeacherRepository teacherRepository,
        IDomainEventRepository domainEventRepository)
    {
        _teacherRepository = teacherRepository;
        _domainEventRepository = domainEventRepository;
    }

    public async Task<TeacherDto> Handle(
        UpdateTeacherCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Load teacher by ID — throw 404 if not found (Req 2.4)
        var teacher = await _teacherRepository.GetByIdAsync(request.TeacherId, cancellationToken);
        if (teacher is null)
            throw new NotFoundException($"Teacher with ID '{request.TeacherId}' was not found.");

        // 2. Check email uniqueness if the email is being changed (Req 2.3)
        if (request.Email is not null &&
            !string.Equals(request.Email, teacher.Email, StringComparison.OrdinalIgnoreCase))
        {
            var emailOwner = await _teacherRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (emailOwner is not null && emailOwner.Id != teacher.Id)
                throw new ConflictException($"A teacher with email '{request.Email}' already exists.");
        }

        // 3. Apply updates — raises TeacherUpdated domain event (Req 2.1, 10.2)
        teacher.Update(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            request.DateOfBirth,
            request.Address);

        // 4. Persist domain events
        await _domainEventRepository.PersistAsync(teacher.DomainEvents, cancellationToken);

        // 5. Persist the updated teacher aggregate
        await _teacherRepository.UpdateAsync(teacher, cancellationToken);

        // 6. Return updated DTO (Req 2.5)
        return teacher.ToDto();
    }
}
