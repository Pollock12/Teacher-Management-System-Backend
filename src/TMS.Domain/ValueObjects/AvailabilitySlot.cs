namespace TMS.Domain.ValueObjects;

public sealed class AvailabilitySlot
{
    public DayOfWeek DayOfWeek { get; }
    public TimeOnly StartTime { get; }
    public TimeOnly EndTime { get; }

    // Precondition: startTime must be strictly earlier than endTime
    public AvailabilitySlot(DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime)
    {
        if (startTime >= endTime)
            throw new ArgumentException("StartTime must be earlier than EndTime.");

        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
    }

    // Returns true when this slot overlaps with the given [start, end) range on the same day
    public bool Overlaps(DayOfWeek day, TimeOnly start, TimeOnly end) =>
        DayOfWeek == day && StartTime < end && EndTime > start;
}
