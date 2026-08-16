namespace TMS.Application.Teachers.DTOs;

public record ScheduleEntryDto(Guid CourseId, DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime);
