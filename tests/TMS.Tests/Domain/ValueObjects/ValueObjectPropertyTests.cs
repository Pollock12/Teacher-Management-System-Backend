using FsCheck;
using FsCheck.Xunit;
using TMS.Domain.ValueObjects;

namespace TMS.Tests.Domain.ValueObjects;

/// <summary>
/// Custom arbitrary for generating valid TimeOnly pairs where start &lt; end,
/// as well as non-overlapping and overlapping pairs of time ranges.
/// FsCheck does not natively generate TimeOnly, so we generate from the
/// underlying ticks (0..TimeOnly.MaxValue.Ticks).
/// </summary>
public static class TimeOnlyArbitrary
{
    // Generates a single TimeOnly from a non-negative long tick value
    public static Arbitrary<TimeOnly> TimeOnly() =>
        Arb.From(
            Gen.Choose(0, (int)(System.TimeOnly.MaxValue.Ticks / TimeSpan.TicksPerSecond))
               .Select(seconds => new TimeOnly(TimeSpan.FromSeconds(seconds).Ticks))
        );

    // Generates a valid (start, end) pair where start < end
    public static Arbitrary<(TimeOnly Start, TimeOnly End)> ValidTimePair() =>
        Arb.From(
            from startSeconds in Gen.Choose(0, (int)(System.TimeOnly.MaxValue.Ticks / TimeSpan.TicksPerSecond) - 1)
            from endSeconds   in Gen.Choose(startSeconds + 1, (int)(System.TimeOnly.MaxValue.Ticks / TimeSpan.TicksPerSecond))
            select (new TimeOnly(TimeSpan.FromSeconds(startSeconds).Ticks),
                    new TimeOnly(TimeSpan.FromSeconds(endSeconds).Ticks))
        );

    // Generates two non-overlapping ranges: [s1, e1) then [s2, e2) where e1 <= s2
    public static Arbitrary<(TimeOnly S1, TimeOnly E1, TimeOnly S2, TimeOnly E2)> NonOverlappingPairs() =>
        Arb.From(
            from s1Secs in Gen.Choose(0, 40000)
            from e1Secs in Gen.Choose(s1Secs + 1, 43199)      // e1 at most one second before max
            from s2Secs in Gen.Choose(e1Secs, 43199)           // s2 >= e1 (touching or later)
            from e2Secs in Gen.Choose(s2Secs + 1, 86399)       // e2 > s2
            select (new TimeOnly(TimeSpan.FromSeconds(s1Secs).Ticks),
                    new TimeOnly(TimeSpan.FromSeconds(e1Secs).Ticks),
                    new TimeOnly(TimeSpan.FromSeconds(s2Secs).Ticks),
                    new TimeOnly(TimeSpan.FromSeconds(e2Secs).Ticks))
        );

    // Generates two overlapping ranges where the first range overlaps the second
    // [s1, e1) overlaps [s2, e2) iff s1 < e2 && e1 > s2
    // We construct: s1 < e2 and e1 > s2, both on the same day
    public static Arbitrary<(TimeOnly S1, TimeOnly E1, TimeOnly S2, TimeOnly E2)> OverlappingPairs() =>
        Arb.From(
            from s1Secs  in Gen.Choose(0, 40000)
            from e1Secs  in Gen.Choose(s1Secs + 2, 43200)     // e1 > s1
            from s2Secs  in Gen.Choose(s1Secs, e1Secs - 1)    // s2 in [s1, e1) → guarantees overlap
            from e2Secs  in Gen.Choose(s2Secs + 1, 86399)     // e2 > s2
            select (new TimeOnly(TimeSpan.FromSeconds(s1Secs).Ticks),
                    new TimeOnly(TimeSpan.FromSeconds(e1Secs).Ticks),
                    new TimeOnly(TimeSpan.FromSeconds(s2Secs).Ticks),
                    new TimeOnly(TimeSpan.FromSeconds(e2Secs).Ticks))
        );
}

/// <summary>
/// Property-based tests for AvailabilitySlot and ScheduleEntry value objects.
/// </summary>
public class ValueObjectPropertyTests
{
    // ─── Property 4: Availability Slot Validity ───────────────────────────
    // Validates: Requirements 7.3

    /// <summary>
    /// For any successfully constructed AvailabilitySlot, StartTime &lt; EndTime always holds.
    /// </summary>
    [Property(DisplayName = "Property 4a: Valid AvailabilitySlot always has StartTime < EndTime")]
    public Property AvailabilitySlot_ValidSlot_StartTimeAlwaysLessThanEndTime()
    {
        var arb = TimeOnlyArbitrary.ValidTimePair();
        return Prop.ForAll(arb, pair =>
        {
            var (start, end) = pair;
            var day = DayOfWeek.Monday; // day doesn't affect this property

            var slot = new AvailabilitySlot(day, start, end);

            return slot.StartTime < slot.EndTime;
        });
    }

    /// <summary>
    /// Construction with StartTime == EndTime always throws ArgumentException.
    /// </summary>
    [Property(DisplayName = "Property 4b: AvailabilitySlot with StartTime == EndTime always throws")]
    public Property AvailabilitySlot_EqualTimes_AlwaysThrows()
    {
        var arb = TimeOnlyArbitrary.TimeOnly();
        return Prop.ForAll(arb, time =>
        {
            var day = DayOfWeek.Tuesday;
            try
            {
                _ = new AvailabilitySlot(day, time, time);
                return false.Label($"Expected ArgumentException for equal times ({time}) but none was thrown");
            }
            catch (ArgumentException)
            {
                return true.ToProperty();
            }
        });
    }

    /// <summary>
    /// Construction with StartTime > EndTime always throws ArgumentException.
    /// </summary>
    [Property(DisplayName = "Property 4c: AvailabilitySlot with StartTime > EndTime always throws")]
    public Property AvailabilitySlot_StartAfterEnd_AlwaysThrows()
    {
        var arb = TimeOnlyArbitrary.ValidTimePair();
        return Prop.ForAll(arb, pair =>
        {
            var (start, end) = pair;
            var day = DayOfWeek.Wednesday;
            // Swap: end becomes start, start becomes end → start > end
            try
            {
                _ = new AvailabilitySlot(day, end, start);
                return false.Label($"Expected ArgumentException when start({end}) > end({start}) but none was thrown");
            }
            catch (ArgumentException)
            {
                return true.ToProperty();
            }
        });
    }

    // ─── Property 3: Schedule Conflict Prevention ─────────────────────────
    // Validates: Requirements 8.3

    /// <summary>
    /// For any two non-overlapping time ranges on the same day, ConflictsWith returns false.
    /// </summary>
    [Property(DisplayName = "Property 3a: Non-overlapping ranges on same day - ConflictsWith returns false")]
    public Property ScheduleEntry_NonOverlappingRanges_SameDay_ReturnsFalse()
    {
        var arb = TimeOnlyArbitrary.NonOverlappingPairs();
        return Prop.ForAll(arb, ranges =>
        {
            var (s1, e1, s2, e2) = ranges;
            var day = DayOfWeek.Friday;
            var courseId = Guid.NewGuid();

            var entry = new ScheduleEntry(courseId, day, s1, e1);

            // Non-overlapping: e1 <= s2, so [s1,e1) and [s2,e2) do not overlap
            bool conflicts = entry.ConflictsWith(day, s2, e2);

            return (!conflicts).Label(
                $"Expected no conflict for non-overlapping [{s1}-{e1}) and [{s2}-{e2}) but got conflict=true");
        });
    }

    /// <summary>
    /// For any two non-overlapping ranges on different days, ConflictsWith returns false.
    /// </summary>
    [Property(DisplayName = "Property 3b: Any ranges on different days - ConflictsWith returns false")]
    public Property ScheduleEntry_DifferentDays_AlwaysReturnsFalse()
    {
        var pairArb = TimeOnlyArbitrary.ValidTimePair();
        return Prop.ForAll(pairArb, pairArb, (pair1, pair2) =>
        {
            var (s1, e1) = pair1;
            var (s2, e2) = pair2;
            var courseId = Guid.NewGuid();

            var entry = new ScheduleEntry(courseId, DayOfWeek.Monday, s1, e1);

            // Different day: ConflictsWith should always return false
            bool conflicts = entry.ConflictsWith(DayOfWeek.Tuesday, s2, e2);

            return (!conflicts).Label(
                $"Expected no conflict for different days but got conflict=true");
        });
    }

    /// <summary>
    /// For any two overlapping time ranges on the same day, ConflictsWith returns true.
    /// </summary>
    [Property(DisplayName = "Property 3c: Overlapping ranges on same day - ConflictsWith returns true")]
    public Property ScheduleEntry_OverlappingRanges_SameDay_ReturnsTrue()
    {
        var arb = TimeOnlyArbitrary.OverlappingPairs();
        return Prop.ForAll(arb, ranges =>
        {
            var (s1, e1, s2, e2) = ranges;
            var day = DayOfWeek.Thursday;
            var courseId = Guid.NewGuid();

            var entry = new ScheduleEntry(courseId, day, s1, e1);

            bool conflicts = entry.ConflictsWith(day, s2, e2);

            return conflicts.Label(
                $"Expected conflict for overlapping [{s1}-{e1}) and [{s2}-{e2}) but got conflict=false");
        });
    }

    /// <summary>
    /// ScheduleEntry also enforces StartTime &lt; EndTime; invalid construction always throws.
    /// </summary>
    [Property(DisplayName = "Property 4d: ScheduleEntry with StartTime >= EndTime always throws")]
    public Property ScheduleEntry_InvalidTimes_AlwaysThrows()
    {
        var arb = TimeOnlyArbitrary.TimeOnly();
        return Prop.ForAll(arb, time =>
        {
            var courseId = Guid.NewGuid();
            try
            {
                _ = new ScheduleEntry(courseId, DayOfWeek.Monday, time, time);
                return false.Label($"Expected ArgumentException for equal times ({time}) but none was thrown");
            }
            catch (ArgumentException)
            {
                return true.ToProperty();
            }
        });
    }
}
