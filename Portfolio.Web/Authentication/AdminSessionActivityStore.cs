using System.Collections.Concurrent;

namespace Portfolio.Web.Authentication;

public sealed class AdminSessionActivityStore
{
    private readonly ConcurrentDictionary<string, SessionActivity> sessions = new(StringComparer.Ordinal);

    public void Start(string sessionId, DateTimeOffset expiresAt)
    {
        sessions[sessionId] = new(expiresAt, DateTimeOffset.UtcNow);
        foreach (var entry in sessions)
        {
            if (entry.Value.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                sessions.TryRemove(entry.Key, out _);
            }
        }
    }

    public DateTimeOffset GetExpiration(string sessionId, DateTimeOffset fallback) =>
        sessions.TryGetValue(sessionId, out var session) ? session.ExpiresAt : fallback;

    public bool HasExpired(string sessionId) =>
        sessions.TryGetValue(sessionId, out var session) && session.ExpiresAt <= DateTimeOffset.UtcNow;

    public bool IsActive(string sessionId, DateTimeOffset now) =>
        sessions.TryGetValue(sessionId, out var session) && now < session.ExpiresAt;

    public bool TryGetExpiration(string sessionId, out DateTimeOffset expiresAt)
    {
        if (sessions.TryGetValue(sessionId, out var session))
        {
            expiresAt = session.ExpiresAt;
            return true;
        }

        expiresAt = default;
        return false;
    }

    public bool TryRenew(string sessionId, DateTimeOffset now, TimeSpan minimumInterval, out DateTimeOffset expiresAt)
    {
        expiresAt = default;
        while (sessions.TryGetValue(sessionId, out var session))
        {
            if (now >= session.ExpiresAt || now - session.LastActivity < minimumInterval)
            {
                return false;
            }

            var renewed = new SessionActivity(now.Add(AdminSession.Duration), now);
            if (sessions.TryUpdate(sessionId, renewed, session))
            {
                expiresAt = renewed.ExpiresAt;
                return true;
            }
        }

        return false;
    }

    public void Remove(string sessionId) => sessions.TryRemove(sessionId, out _);

    private sealed record SessionActivity(DateTimeOffset ExpiresAt, DateTimeOffset LastActivity);
}
