using Cypherly.Identity.Infrastructure.Persistence.Outbox;

namespace Cypherly.Identity.Infrastructure.Persistence.Repositories;

public interface IOutboxRepository
{
    Task<OutboxMessage[]> GetUnprocessedAsync(int batchSize);
    Task MarkAsProcessedAsync(OutboxMessage message);
}