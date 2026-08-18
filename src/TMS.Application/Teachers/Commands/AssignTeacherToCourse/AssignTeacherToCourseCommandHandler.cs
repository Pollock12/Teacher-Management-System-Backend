using MediatR;
using TMS.Domain.Exceptions;
using TMS.Domain.Repositories;

namespace TMS.Application.Teachers.Commands.AssignTeacherToCourse;

/// <summary>
/// Handles <see cref="AssignTeacherToCourseCommand"/>.
/// Satisfies Requirements 8.1–8.5, 9.4, 10.5.
/// </summary>
public sealed class AssignTeacherToCourseCommandHandler : IRequestHandler<AssignTeacherToCourseCommand>
{
    private readonly ITeacherRepository _teacherRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IDomainEventRepository _domainEventRepository;

    public AssignTeacherToCourseCommandHandler(
        ITeacherRepository teacherRepository,
        ICourseRepository courseRepository,
        IDomainEventRepository domainEventRepository)
    {
        _teacherRepository = teacherRepository;
        _courseRepository = courseRepository;
        _domainEventRepository = domainEventRepository;
    }

    public async Task Handle(AssignTeacherToCourseCommand request, CancellationToken cancellationToken)
    {
        // 1. Load teacher by ID — throw NotFoundException if not found (Req 8.2, 9.4)
        var teacher = await _teacherRepository.GetByIdAsync(request.TeacherId, cancellationToken);
        if (teacher is null)
            throw new NotFoundException($"Teacher with ID '{request.TeacherId}' was not found.");

        // 2. Load course by ID — throw NotFoundException if not found (Req 8.3)
        var course = await _courseRepository.GetByIdAsync(request.CourseId, cancellationToken);
        if (course is null)
            throw new NotFoundException($"Course with ID '{request.CourseId}' was not found.");

        // 3. Assign teacher to course — throws DomainRuleException internally on schedule conflict (Req 8.4, 8.5)
        teacher.AssignToCourse(request.CourseId, request.DayOfWeek, request.StartTime, request.EndTime);

        // 4. Persist domain events (TeacherAssignedToCourse event) (Req 10.5)
        await _domainEventRepository.PersistAsync(teacher.DomainEvents, cancellationToken);

        // 5. Clear domain events from the aggregate
        teacher.ClearDomainEvents();

        // 6. Persist the updated teacher aggregate (Req 8.1)
        await _teacherRepository.UpdateAsync(teacher, cancellationToken);
    }
}
