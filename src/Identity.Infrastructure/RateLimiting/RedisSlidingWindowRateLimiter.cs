using Identity.Application.Contracts.RateLimiting;
using StackExchange.Redis;

namespace Identity.Infrastructure.RateLimiting;

/// <summary>
/// Sliding-window rate limiter backed by a Redis/Valkey sorted set, safe across multiple app replicas.
/// Each allowed call is recorded as a member scored by its timestamp; the Lua script atomically
/// evicts entries outside the window, counts what remains, and only admits the new call if under the limit.
/// </summary>
public class RedisSlidingWindowRateLimiter(IConnectionMultiplexer connectionMultiplexer) : IRateLimiter
{
    private const string Script = """
                                   local key = KEYS[1]
                                   local now = tonumber(ARGV[1])
                                   local windowMs = tonumber(ARGV[2])
                                   local limit = tonumber(ARGV[3])
                                   local member = ARGV[4]

                                   redis.call('ZREMRANGEBYSCORE', key, '-inf', now - windowMs)

                                   local count = redis.call('ZCARD', key)
                                   if count >= limit then
                                       return 0
                                   end

                                   redis.call('ZADD', key, now, member)
                                   redis.call('PEXPIRE', key, windowMs)
                                   return 1
                                   """;

    public async Task<bool> IsAllowedAsync(string key, int limit, TimeSpan window, CancellationToken cancellationToken)
    {
        var db = connectionMultiplexer.GetDatabase();

        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var member = $"{now}-{Guid.NewGuid()}";

        var result = await db.ScriptEvaluateAsync(
            Script,
            [(RedisKey)key],
            [(RedisValue)now, (RedisValue)(long)window.TotalMilliseconds, (RedisValue)limit, (RedisValue)member]);

        return (long)result == 1;
    }
}
