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

        // It's converting Domain Event to DomainEventDocument
        var documents = eventList.Select(domainEvent => new DomainEventDocument
        {
            Id          = Guid.NewGuid(),
            EventType   = domainEvent.EventType,
            OccurredAt  = domainEvent.OccurredAt,
            //It converts the C# event object into JSON text
            // domainEvent -> TeacherCreated object -> GetType() -> TeacherCreated
            Payload     = JsonSerializer.Serialize(domainEvent, domainEvent.GetType())
        }).ToList();

        // This is called a bulk insert
        await _context.DomainEvents.InsertManyAsync(documents, cancellationToken: ct);
    }
}

/*
  Teacher created -> TeacherCreated event created -> Event stored inside Teacher.DomainEvents -> DomainEventRepository receives the event 
  -> Converts event → DomainEventDocument -> Converts event data → JSON -> InsertManyAsync() -> MongoDB -> domain_events collection

  DomainEventRepository takes important events that happened in your Domain layer, converts them into MongoDB documents, 
  and stores them as an event history in the domain_events collection.

*/
