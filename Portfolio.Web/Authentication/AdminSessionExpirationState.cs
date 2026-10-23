namespace Portfolio.Web.Authentication;

public sealed class AdminSessionExpirationState
{
    private readonly SemaphoreSlim expirationLock = new(1, 1);
    private int expired;

    public event Action? Expired;

    public bool IsExpired => Volatile.Read(ref expired) == 1;

    public bool HasUnsavedChanges { get; private set; }

    public Func<CancellationToken, ValueTask<bool>>? UnsavedChangesProbe { get; set; }

    public async Task MarkExpiredAsync(CancellationToken cancellationToken)
    {
        await expirationLock.WaitAsync(cancellationToken);
        try
        {
            if (IsExpired)
            {
                return;
            }

            var probe = UnsavedChangesProbe;
            HasUnsavedChanges = probe is not null && await probe(cancellationToken);
            Volatile.Write(ref expired, 1);
        }
        finally
        {
            expirationLock.Release();
        }

        Expired?.Invoke();
    }
}
