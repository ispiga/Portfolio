namespace Portfolio.Web.Authentication;

public sealed class AdminSessionExpirationState
{
    private int expired;

    public event Action? Expired;

    public bool IsExpired => Volatile.Read(ref expired) == 1;

    public void MarkExpired()
    {
        if (Interlocked.Exchange(ref expired, 1) == 0)
        {
            Expired?.Invoke();
        }
    }
}
