using Grpc.Core;
using Grpc.Core.Interceptors;
using MariesWonderland.Extensions;
using System.Collections.Concurrent;

namespace MariesWonderland.Accounts;

/// <summary>
/// What the dashboard shows about traffic: when each player last made a call, and calls and failures per minute
/// over the last hour. Last-seen times are written to the account records about once a minute.
/// </summary>
public sealed class ActivityTracker(TimeProvider timeProvider)
{
    private const int Minutes = 60;

    private readonly ConcurrentDictionary<long, DateTime> _lastSeen = new();
    private readonly ConcurrentDictionary<long, DateTime> _unsaved = new();
    private readonly long[] _calls = new long[Minutes];
    private readonly long[] _failures = new long[Minutes];
    private readonly long[] _bucketMinute = new long[Minutes];
    private readonly Lock _bucketLock = new();

    public DateTime StartedAt { get; } = timeProvider.GetUtcNow().UtcDateTime;

    public void Record(long? userId, bool failed)
    {
        DateTime now = timeProvider.GetUtcNow().UtcDateTime;
        if (userId is long id)
        {
            _lastSeen[id] = now;
            _unsaved[id] = now;
        }

        long minute = now.Ticks / TimeSpan.TicksPerMinute;
        int slot = (int)(minute % Minutes);
        lock (_bucketLock)
        {
            if (_bucketMinute[slot] != minute)
            {
                _bucketMinute[slot] = minute;
                _calls[slot] = 0;
                _failures[slot] = 0;
            }
            _calls[slot]++;
            if (failed)
            {
                _failures[slot]++;
            }
        }
    }

    /// <summary>Seeds last-seen times from the account records at startup.</summary>
    public void Seed(IEnumerable<(long UserId, DateTime At)> lastSeen)
    {
        foreach ((long userId, DateTime at) in lastSeen)
        {
            _lastSeen.TryAdd(userId, at);
        }
    }

    public DateTime? LastSeen(long userId) => _lastSeen.TryGetValue(userId, out DateTime at) ? at : null;

    /// <summary>How many players made a call within <paramref name="window"/>.</summary>
    public int ActiveWithin(TimeSpan window)
    {
        DateTime cutoff = timeProvider.GetUtcNow().UtcDateTime - window;
        return _lastSeen.Values.Count(at => at >= cutoff);
    }

    /// <summary>Calls and failures per minute, oldest first, for the last <paramref name="minutes"/> minutes.</summary>
    public IReadOnlyList<(DateTime Minute, long Calls, long Failures)> PerMinute(int minutes = Minutes)
    {
        long current = timeProvider.GetUtcNow().UtcDateTime.Ticks / TimeSpan.TicksPerMinute;
        List<(DateTime, long, long)> result = [];
        lock (_bucketLock)
        {
            for (long minute = current - Math.Min(minutes, Minutes) + 1; minute <= current; minute++)
            {
                int slot = (int)(minute % Minutes);
                bool valid = _bucketMinute[slot] == minute;
                result.Add((new DateTime(minute * TimeSpan.TicksPerMinute, DateTimeKind.Utc), valid ? _calls[slot] : 0, valid ? _failures[slot] : 0));
            }
        }
        return result;
    }

    /// <summary>The last-seen times recorded since the previous call.</summary>
    public IReadOnlyDictionary<long, DateTime> TakeUnsaved()
    {
        Dictionary<long, DateTime> taken = [];
        foreach (long userId in _unsaved.Keys)
        {
            if (_unsaved.TryRemove(userId, out DateTime at))
            {
                taken[userId] = at;
            }
        }
        return taken;
    }
}

/// <summary>Counts every gRPC call for <see cref="ActivityTracker"/>. Sits right after the session interceptor.</summary>
public sealed class ActivityInterceptor(ActivityTracker tracker) : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        long? userId = context.TryGetUserId(out long id) ? id : null;
        try
        {
            TResponse response = await continuation(request, context);
            tracker.Record(userId, failed: false);
            return response;
        }
        catch
        {
            tracker.Record(userId, failed: true);
            throw;
        }
    }
}

/// <summary>Writes last-seen times to the account records once a minute (and on shutdown).</summary>
public sealed class ActivityFlusher(ActivityTracker tracker, AccountStore accounts, ILogger<ActivityFlusher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            tracker.Seed(accounts.All().Values.Where(a => a.LastSeenAt != null).Select(a => (a.UserId, DateTime.SpecifyKind(a.LastSeenAt!.Value, DateTimeKind.Utc))));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Couldn't read last-seen times");
        }

        using PeriodicTimer timer = new(TimeSpan.FromMinutes(1));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                Flush();
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        Flush();
    }

    private void Flush()
    {
        try
        {
            accounts.SaveLastSeen(tracker.TakeUnsaved());
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Couldn't save last-seen times");
        }
    }
}
