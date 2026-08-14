using TMS.Domain.Common;

namespace TMS.Domain.Repositories;

public interface IDomainEventRepository
{
    //Provide a way to save a collection of domain events.
    //PersistAsync -> Save these events somewhere asynchronously
    Task PersistAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default);
}
