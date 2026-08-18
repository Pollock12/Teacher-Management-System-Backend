using MediatR;
using TMS.Domain.Exceptions;
using TMS.Domain.Repositories;

namespace TMS.Application.Teachers.Commands.RemoveTeacherFromCourse;

/// <summary>
/// Handles <see cref="RemoveTeacherFromCourseCommand"/>.
/// Satisfies Requirement 8.6.
/// </summary>
public sealed class RemoveTeacherFromCourseCommandHandler : IRequestHandler<RemoveTeacherFromCourseCommand>
{
    private readonly ITeacherRepository _teacherRepository;

    public RemoveTeacherFromCourseCommandHandler(ITeacherRepository teacherRepository)
    {
        _teacherRepository = teacherRepository;
    }

    public async Task Handle(RemoveTeacherFromCourseCommand request, CancellationToken cancellationToken)
    {
        // 1. Load teacher by ID — throw NotFoundException if not found (Req 8.6)
        var teacher = await _teacherRepository.GetByIdAsync(request.TeacherId, cancellationToken);
        if (teacher is null)
            throw new NotFoundException($"Teacher with ID '{request.TeacherId}' was not found.");

        // 2. Remove the course from the teacher — domain method throws NotFoundException
        //    internally if the course is not assigned to this teacher (Req 8.6)
        teacher.RemoveFromCourse(request.CourseId);

        // 3. Persist the updated teacher aggregate (Req 8.6)
        await _teacherRepository.UpdateAsync(teacher, cancellationToken);
    }
}
