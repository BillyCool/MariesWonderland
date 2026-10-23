using System.Collections.Concurrent;

namespace MariesWonderland.Data;

/// <summary>
/// One lock per user, so requests for the same user run one at a time while different users run in parallel.
/// A request holds its caller's lock from start to finish (<c>UserLockInterceptor</c>); code that needs another
/// user, or a user before login, takes that user's lock with a timeout so two requests waiting on each other
/// give up instead of hanging. Not reentrant: callers check whether their request already holds the user.
/// </summary>
public sealed class UserLocks
{
    private readonly ConcurrentDictionary<long, SemaphoreSlim> _locks = new();

    /// <summary>Waits for the user's lock; dispose the result to release it.</summary>
    public async Task<IDisposable> AcquireAsync(long userId, CancellationToken cancellationToken = default)
    {
        SemaphoreSlim gate = Gate(userId);
        await gate.WaitAsync(cancellationToken);
        return new Releaser(gate);
    }

    /// <summary>Waits up to <paramref name="timeout"/> for the user's lock; null when it wasn't free in time.</summary>
    public IDisposable? TryAcquire(long userId, TimeSpan timeout)
    {
        SemaphoreSlim gate = Gate(userId);
        return gate.Wait(timeout) ? new Releaser(gate) : null;
    }

    private SemaphoreSlim Gate(long userId) => _locks.GetOrAdd(userId, static _ => new SemaphoreSlim(1, 1));

    private sealed class Releaser(SemaphoreSlim gate) : IDisposable
    {
        private int _released;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _released, 1) == 0)
            {
                gate.Release();
            }
        }
    }
}
