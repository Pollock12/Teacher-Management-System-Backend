namespace TMS.Application.Courses.DTOs;

public record CourseDto(
    Guid Id,
    string Name,
    string? Description,
    Guid SubjectId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

// Course is your domain object. 
// CourseDto is the data representation of that course that you want to transfer to the outside world.
