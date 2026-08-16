namespace TMS.Application.Subjects.DTOs;

public record SubjectDto(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
