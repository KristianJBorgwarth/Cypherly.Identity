using Identity.Infrastructure.Persistence.Outbox;

namespace Identity.Infrastructure.Persistence.Repositories;

public interface IOutboxRepository
{
    Task<OutboxMessage[]> GetUnprocessedAsync(int batchSize);
    Task MarkAsProcessedAsync(OutboxMessage message);
}