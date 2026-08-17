using TMS.Domain.Aggregates.Teachers;

namespace TMS.Application.Teachers.DTOs;

/// <summary>
/// Extension methods for mapping <see cref="Teacher"/> domain objects to their
/// corresponding DTOs. Keeps mapping logic in one place so handlers stay lean.
/// </summary>

// This class is responsible for converting your Domain Teacher object into DTOs before sending data to the API/frontend.

public static class TeacherMappingExtensions
{
    /// <summary>
    /// Maps a <see cref="Teacher"/> aggregate to the full <see cref="TeacherDto"/>
    /// that includes all subject assignments, availability slots, and schedule entries.
    /// </summary>
    public static TeacherDto ToDto(this Teacher teacher) =>
        new(
            Id: teacher.Id,
            FirstName: teacher.FirstName,
            LastName: teacher.LastName,
            Email: teacher.Email,
            PhoneNumber: teacher.PhoneNumber,
            DateOfBirth: teacher.DateOfBirth,
            Address: teacher.Address,
            CreatedAt: teacher.CreatedAt,
            UpdatedAt: teacher.UpdatedAt,
            SubjectAssignments: teacher.SubjectAssignments
                .Select(a => new SubjectAssignmentDto(a.SubjectId, a.AssignedAt))
                .ToList(),
            AvailabilitySlots: teacher.AvailabilitySlots
                .Select(s => new AvailabilitySlotDto(s.DayOfWeek, s.StartTime, s.EndTime))
                .ToList(),
            ScheduleEntries: teacher.ScheduleEntries
                .Select(e => new ScheduleEntryDto(e.CourseId, e.DayOfWeek, e.StartTime, e.EndTime))
                .ToList()
        );

    /// <summary>
    /// Maps a <see cref="Teacher"/> aggregate to the lightweight <see cref="TeacherSummaryDto"/>
    /// used in paginated list responses.
    /// </summary>
    public static TeacherSummaryDto ToSummaryDto(this Teacher teacher) =>
        new(
            Id: teacher.Id,
            FirstName: teacher.FirstName,
            LastName: teacher.LastName,
            Email: teacher.Email
        );
}
