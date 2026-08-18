using MediatR;
using TMS.Application.Teachers.DTOs;

namespace TMS.Application.Teachers.Commands.UpdateTeacher;

/// <summary>
/// Command to update an existing teacher's details.
/// Satisfies Requirements 2.1–2.5.
/// </summary>
public record UpdateTeacherCommand(
    Guid TeacherId,
    string? FirstName,
    string? LastName,
    string? Email,
    string? PhoneNumber,
    DateOnly? DateOfBirth,
    string? Address
) : IRequest<TeacherDto>;
