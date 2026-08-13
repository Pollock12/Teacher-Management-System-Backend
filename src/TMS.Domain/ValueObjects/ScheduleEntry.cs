namespace TMS.Domain.ValueObjects;

public sealed class ScheduleEntry
{
    public Guid CourseId { get; }
    public DayOfWeek DayOfWeek { get; }
    public TimeOnly StartTime { get; }
    public TimeOnly EndTime { get; }

    // Precondition: startTime must be strictly earlier than endTime
    public ScheduleEntry(Guid courseId, DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime)
    {
        if (startTime >= endTime)
            throw new ArgumentException("StartTime must be earlier than EndTime.");

        CourseId = courseId;
        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
    }

    // Returns true when this schedule entry conflicts with the given time slot
    public bool ConflictsWith(DayOfWeek day, TimeOnly start, TimeOnly end) =>
        DayOfWeek == day && StartTime < end && EndTime > start;
}
