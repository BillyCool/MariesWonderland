using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Interceptors;
using MariesWonderland.Models.Entities;
using MariesWonderland.Proto.User;
using MariesWonderland.Tests.Infrastructure;

namespace MariesWonderland.Tests.Interceptors;

/// <summary>
/// Per-user locks: one user's requests run one at a time, different users in parallel; calls that need another
/// user (or log in) take that user's lock without waiting on themselves.
/// </summary>
public class UserLockInterceptorTests : InterceptorTestBase
{
    private static readonly TimeSpan Wait = TimeSpan.FromSeconds(5);

    /// <summary>Starts a call whose handler blocks until <paramref name="release"/> completes; <paramref name="entered"/> completes once it runs.</summary>
    private static Task<SetUserNameResponse> BlockingCall(UserLockInterceptor interceptor, long userId, TaskCompletionSource entered, Task release)
        => interceptor.UnaryServerHandler(new SetUserNameRequest(), ContextFor(userId), async (_, _) =>
        {
            entered.TrySetResult();
            await release;
            return new SetUserNameResponse();
        });

    [Fact]
    public async Task SameUser_SecondCallWaitsForTheFirst()
    {
        UserLockInterceptor interceptor = new(CreateEmptyStore());
        TaskCompletionSource firstIn = new(), secondIn = new(), releaseFirst = new();

        Task<SetUserNameResponse> first = BlockingCall(interceptor, 1, firstIn, releaseFirst.Task);
        await firstIn.Task.WaitAsync(Wait, TestContext.Current.CancellationToken);
        Task<SetUserNameResponse> second = BlockingCall(interceptor, 1, secondIn, Task.CompletedTask);

        await Task.Delay(100, TestContext.Current.CancellationToken);
        Assert.False(secondIn.Task.IsCompleted);

        releaseFirst.SetResult();
        await Task.WhenAll(first, second).WaitAsync(Wait, TestContext.Current.CancellationToken);
        Assert.True(secondIn.Task.IsCompleted);
    }

    [Fact]
    public async Task DifferentUsers_RunInParallel()
    {
        UserLockInterceptor interceptor = new(CreateEmptyStore());
        TaskCompletionSource firstIn = new(), secondIn = new(), release = new();

        Task<SetUserNameResponse> first = BlockingCall(interceptor, 1, firstIn, release.Task);
        Task<SetUserNameResponse> second = BlockingCall(interceptor, 2, secondIn, release.Task);

        await Task.WhenAll(firstIn.Task, secondIn.Task).WaitAsync(Wait, TestContext.Current.CancellationToken);
        release.SetResult();
        await Task.WhenAll(first, second).WaitAsync(Wait, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CallsWithoutASession_AreNotLocked()
    {
        UserDataStore store = CreateEmptyStore();
        UserLockInterceptor interceptor = new(store);
        using IDisposable held = store.Locks.TryAcquire(1, TimeSpan.Zero)!;

        SetUserNameResponse response = await interceptor.UnaryServerHandler(new SetUserNameRequest(), FakeServerCallContext.Anonymous(),
            (_, _) => Task.FromResult(new SetUserNameResponse())).WaitAsync(Wait, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
    }

    [Fact]
    public async Task ParallelCallsForOneUser_DontLoseChanges()
    {
        UserDataStore store = CreateStoreWithUser(1, new DarkUserMemoryDatabase());
        UserLockInterceptor interceptor = new(store);

        Task[] calls = [.. Enumerable.Range(0, 200).Select(i => Task.Run(() => interceptor.UnaryServerHandler(new SetUserNameRequest(), ContextFor(1), async (_, ctx) =>
        {
            DarkUserMemoryDatabase userDb = store.GetOrCreate(ctx.GetUserId());
            int count = userDb.EntityIUserMaterial.Count;
            await Task.Yield();
            userDb.EntityIUserMaterial.Add(new EntityIUserMaterial { UserId = 1, MaterialId = count + 1, Count = 1 });
            return new SetUserNameResponse();
        })))];
        await Task.WhenAll(calls).WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);

        Assert.True(store.TryGet(1, out DarkUserMemoryDatabase userDb));
        Assert.Equal(Enumerable.Range(1, 200), userDb.EntityIUserMaterial.Select(m => m.MaterialId));
    }

    [Fact]
    public async Task TryLockUser_IsANoOpForTheCallersOwnUserAndWaitsForOthers()
    {
        UserDataStore store = CreateEmptyStore();
        UserLockInterceptor interceptor = new(store);
        using IDisposable otherHeld = store.Locks.TryAcquire(2, TimeSpan.Zero)!;

        bool gotOwn = false, gotOther = true;
        await interceptor.UnaryServerHandler(new SetUserNameRequest(), ContextFor(1), (_, ctx) =>
        {
            using IDisposable? own = ctx.TryLockUser(store, 1);
            // A short wait stands in for the real timeout: user 2 is busy for the whole call.
            using IDisposable? other = store.Locks.TryAcquire(2, TimeSpan.FromMilliseconds(50));
            gotOwn = own != null;
            gotOther = other != null;
            return Task.FromResult(new SetUserNameResponse());
        }).WaitAsync(Wait, TestContext.Current.CancellationToken);

        Assert.True(gotOwn);
        Assert.False(gotOther);
    }

    [Fact]
    public void LockUser_WaitsForAnotherUsersLock()
    {
        UserDataStore store = CreateEmptyStore();
        using IDisposable held = store.Locks.TryAcquire(2, TimeSpan.Zero)!;

        // Released well before the timeout runs out, so the call gets the lock.
        Task release = Task.Run(async () =>
        {
            await Task.Delay(50);
            held.Dispose();
        });
        using (IDisposable acquired = ContextFor(1).LockUser(store, 2))
        {
            Assert.NotNull(acquired);
        }
        release.Wait(TestContext.Current.CancellationToken);
    }

    [Fact]
    public void RegisterOrGetUser_InParallel_GivesOneUserPerDevice()
    {
        UserDataStore store = CreateEmptyStore();
        DarkMasterMemoryDatabase masterDb = new();

        long[] ids = [.. Enumerable.Range(0, 50).AsParallel().Select(_ => store.RegisterOrGetUser("device-1", masterDb).UserId)];

        Assert.Single(ids.Distinct());
        Assert.Single(store.All);
    }
}
