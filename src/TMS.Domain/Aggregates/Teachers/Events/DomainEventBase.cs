namespace TMS.Domain.Aggregates.Teachers.Events;

/// <summary>
/// Abstract base record for all Teacher domain events.
/// Provides default EventId and OccurredAt values.
/// Full implementation is defined in task 3.3.
/// </summary>
public abstract record DomainEventBase : Common.IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public abstract string EventType { get; }
}
