using System.Text.Json;
using TMS.Domain.Common;
using TMS.Domain.Repositories;
using TMS.Infrastructure.Persistence;

namespace TMS.Infrastructure.Persistence.Repositories;

/// <summary>
/// MongoDB implementation of <see cref="IDomainEventRepository"/>.
/// Serialises each domain event to a JSON payload and bulk-inserts
/// all documents into the <c>domain_events</c> collection.
/// </summary>
public sealed class DomainEventRepository : IDomainEventRepository
{
    private readonly MongoDbContext _context;

    public DomainEventRepository(MongoDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task PersistAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default)
    {
        var eventList = events.ToList();

        if (eventList.Count == 0)
            return;

        var documents = eventList.Select(domainEvent => new DomainEventDocument
        {
            Id          = Guid.NewGuid(),
            EventType   = domainEvent.EventType,
            OccurredAt  = domainEvent.OccurredAt,
            Payload     = JsonSerializer.Serialize(domainEvent, domainEvent.GetType())
        }).ToList();

        await _context.DomainEvents.InsertManyAsync(documents, cancellationToken: ct);
    }
}
