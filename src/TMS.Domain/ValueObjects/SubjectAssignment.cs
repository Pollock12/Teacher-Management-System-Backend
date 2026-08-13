namespace TMS.Domain.ValueObjects;

public sealed class SubjectAssignment
{
    public Guid SubjectId { get; }
    public DateTime AssignedAt { get; }

    public SubjectAssignment(Guid subjectId)
    {
        if (subjectId == Guid.Empty)
            throw new ArgumentException("SubjectId cannot be empty.");

        SubjectId = subjectId;
        AssignedAt = DateTime.UtcNow;
    }
}
