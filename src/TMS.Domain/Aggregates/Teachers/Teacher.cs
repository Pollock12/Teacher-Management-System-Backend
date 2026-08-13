using TMS.Domain.Aggregates.Teachers.Events;
using TMS.Domain.Exceptions;
using TMS.Domain.ValueObjects;

namespace TMS.Domain.Aggregates.Teachers;

/// <summary>
/// Teacher aggregate root. Manages a teacher's profile, subject assignments,
/// availability slots, and course schedule entries.
/// </summary>
public sealed class Teacher : Common.Entity
{
    // ── Properties ─────────────────────────────────────────────────────────

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public DateOnly? DateOfBirth { get; private set; }
    public string? Address { get; private set; }
    public bool IsDeleted { get; private set; }

    private readonly List<SubjectAssignment> _subjectAssignments = new();
    public IReadOnlyCollection<SubjectAssignment> SubjectAssignments => _subjectAssignments.AsReadOnly();

    private readonly List<AvailabilitySlot> _availabilitySlots = new();
    public IReadOnlyCollection<AvailabilitySlot> AvailabilitySlots => _availabilitySlots.AsReadOnly();

    private readonly List<ScheduleEntry> _scheduleEntries = new();
    public IReadOnlyCollection<ScheduleEntry> ScheduleEntries => _scheduleEntries.AsReadOnly();

    // Private constructor — only the factory method can create instances
    private Teacher() { }

    // ── Factory Method ──────────────────────────────────────────────────────
    /// <summary>
    /// Creates a new Teacher aggregate.
    /// </summary>
    /// <param name="firstName">Non-empty first name.</param>
    /// <param name="lastName">Non-empty last name.</param>
    /// <param name="email">Non-empty email address.</param>
    /// <param name="phoneNumber">Optional phone number.</param>
    /// <param name="dateOfBirth">Optional date of birth.</param>
    /// <param name="address">Optional address.</param>
    /// <returns>A valid <see cref="Teacher"/> with a new Id and a raised <see cref="TeacherCreated"/> event.</returns>
    public static Teacher Create(
        string firstName,
        string lastName,
        string email,
        string? phoneNumber = null,
        DateOnly? dateOfBirth = null,
        string? address = null)
    {
        var teacher = new Teacher
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = phoneNumber,
            DateOfBirth = dateOfBirth,
            Address = address,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        teacher.AddDomainEvent(new TeacherCreated(teacher.Id, firstName, lastName, email));
        return teacher;
    }

    // ── Update ──────────────────────────────────────────────────────────────
    /// <summary>
    /// Updates any non-null fields on the teacher profile.
    /// At least one field should be non-null for a meaningful update.
    /// Refreshes <see cref="Common.Entity.UpdatedAt"/> and raises <see cref="TeacherUpdated"/>.
    /// </summary>
    public void Update(
        string? firstName,
        string? lastName,
        string? email,
        string? phoneNumber,
        DateOnly? dateOfBirth,
        string? address)
    {
        if (firstName != null) FirstName = firstName;
        if (lastName != null) LastName = lastName;
        if (email != null) Email = email;
        if (phoneNumber != null) PhoneNumber = phoneNumber;
        if (dateOfBirth != null) DateOfBirth = dateOfBirth;
        if (address != null) Address = address;

        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new TeacherUpdated(Id, firstName, lastName, email));
    }

    // ── Soft Delete ─────────────────────────────────────────────────────────
    /// <summary>
    /// Marks the teacher as deleted.
    /// </summary>
    /// <exception cref="DomainRuleException">Thrown when the teacher has active course assignments.</exception>
    public void SoftDelete()
    {
        if (_scheduleEntries.Count > 0)
            throw new DomainRuleException(
                "Teacher has active course assignments and cannot be deleted.");

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new TeacherDeleted(Id));
    }

    // ── Assign Subject ──────────────────────────────────────────────────────
    /// <summary>
    /// Assigns a subject to this teacher.
    /// </summary>
    /// <param name="subjectId">The subject to assign.</param>
    /// <exception cref="ConflictException">Thrown when the subject is already assigned.</exception>
    public void AssignSubject(Guid subjectId)
    {
        if (_subjectAssignments.Any(a => a.SubjectId == subjectId))
            throw new ConflictException($"Subject {subjectId} is already assigned to this teacher.");

        _subjectAssignments.Add(new SubjectAssignment(subjectId));
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new SubjectAssignedToTeacher(Id, subjectId));
    }

    // ── Remove Subject ──────────────────────────────────────────────────────
    /// <summary>
    /// Removes a subject assignment from this teacher.
    /// </summary>
    /// <param name="subjectId">The subject to remove.</param>
    /// <exception cref="NotFoundException">Thrown when the subject is not currently assigned.</exception>
    public void RemoveSubject(Guid subjectId)
    {
        var assignment = _subjectAssignments.FirstOrDefault(a => a.SubjectId == subjectId)
            ?? throw new NotFoundException($"Subject {subjectId} is not assigned to this teacher.");

        _subjectAssignments.Remove(assignment);
        UpdatedAt = DateTime.UtcNow;
    }

    // ── Set Availability ────────────────────────────────────────────────────
    /// <summary>
    /// Replaces the teacher's entire availability schedule with the provided slots.
    /// Pass an empty enumerable to clear all availability.
    /// </summary>
    /// <param name="slots">Non-null collection of <see cref="AvailabilitySlot"/> values.</param>
    public void SetAvailability(IEnumerable<AvailabilitySlot> slots)
    {
        _availabilitySlots.Clear();
        _availabilitySlots.AddRange(slots);
        UpdatedAt = DateTime.UtcNow;
    }

    // ── Assign To Course ────────────────────────────────────────────────────
    /// <summary>
    /// Assigns the teacher to a course at the given time slot.
    /// </summary>
    /// <param name="courseId">The course to assign.</param>
    /// <param name="day">Day of the week for the course.</param>
    /// <param name="startTime">Start time of the course slot.</param>
    /// <param name="endTime">End time of the course slot.</param>
    /// <exception cref="DomainRuleException">Thrown when the time slot conflicts with an existing schedule entry.</exception>
    public void AssignToCourse(Guid courseId, DayOfWeek day, TimeOnly startTime, TimeOnly endTime)
    {
        if (_scheduleEntries.Any(e => e.ConflictsWith(day, startTime, endTime)))
            throw new DomainRuleException(
                "The teacher already has a scheduled course that overlaps this time slot.");

        _scheduleEntries.Add(new ScheduleEntry(courseId, day, startTime, endTime));
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new TeacherAssignedToCourse(Id, courseId, day, startTime, endTime));
    }

    // ── Remove From Course ──────────────────────────────────────────────────
    /// <summary>
    /// Removes the teacher's assignment from a course.
    /// </summary>
    /// <param name="courseId">The course to remove.</param>
    /// <exception cref="NotFoundException">Thrown when no schedule entry exists for the given course.</exception>
    public void RemoveFromCourse(Guid courseId)
    {
        var entry = _scheduleEntries.FirstOrDefault(e => e.CourseId == courseId)
            ?? throw new NotFoundException($"Course {courseId} is not assigned to this teacher.");

        _scheduleEntries.Remove(entry);
        UpdatedAt = DateTime.UtcNow;
    }
}
