using MediatR;
using TMS.Application.Teachers.DTOs;

namespace TMS.Application.Teachers.Commands.CreateTeacher;

/// <summary>
/// Command to create a new teacher.
/// Satisfies Requirements 1.1–1.6.
/// </summary>
public record CreateTeacherCommand(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    DateOnly? DateOfBirth,
    string? Address
) : IRequest<TeacherDto>;

/*
  This request expects a TeacherDto as its response.
  CreateTeacherCommand expects TeacherDto
*/