using MediatR;
using TMS.Application.Courses.DTOs;
using TMS.Domain.Aggregates.Courses;
using TMS.Domain.Exceptions;
using TMS.Domain.Repositories;

namespace TMS.Application.Courses.Commands.CreateCourse;

/// <summary>
/// Handles <see cref="CreateCourseCommand"/>.
/// Satisfies Requirements 8.4, 8.5.
/// </summary>

// One subject can have multiple Courses
public sealed class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, CourseDto>
{
    private readonly ICourseRepository _courseRepository;
    private readonly ISubjectRepository _subjectRepository;

    public CreateCourseCommandHandler(
        ICourseRepository courseRepository,
        ISubjectRepository subjectRepository)
    {
        _courseRepository = courseRepository;
        _subjectRepository = subjectRepository;
    }

    public async Task<CourseDto> Handle(
        CreateCourseCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Verify the referenced subject exists (Req 8.4, 8.5)
        var subject = await _subjectRepository.GetByIdAsync(request.SubjectId, cancellationToken);
        if (subject is null)
            throw new NotFoundException($"Subject with ID '{request.SubjectId}' was not found.");

        // 2. Create the Course aggregate
        var course = Course.Create(request.Name, request.SubjectId, request.Description);

        // 3. Persist the course
        await _courseRepository.AddAsync(course, cancellationToken);

        // 4. Return mapped DTO
        return new CourseDto(
            course.Id,
            course.Name,
            course.Description,
            course.SubjectId,
            course.CreatedAt,
            course.UpdatedAt);
    }
}
