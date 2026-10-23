using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Interceptors;
using MariesWonderland.Models.Entities;
using MariesWonderland.Proto.User;
using MariesWonderland.Tests.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

namespace MariesWonderland.Tests.Interceptors;

public sealed class RequestJournalInterceptorTests : InterceptorTestBase, IDisposable
{
    private const long UserId = 1;
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"mw-journal-{Guid.NewGuid():N}");
    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 10, 24, 12, 3, 45, 123, TimeSpan.Zero));

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, true);
        }
    }

    private RequestJournalInterceptor Journal() => new(NullLogger<RequestJournalInterceptor>.Instance, _time, _directory);

    [Fact]
    public async Task ChangedRows_AreJournaledAsNewRemovedAndOldToNewFields()
    {
        DarkUserMemoryDatabase userDb = new() { UserId = UserId };
        userDb.EntityIUserStatus.Add(new EntityIUserStatus { UserId = UserId, Level = 10, Exp = 100 });
        userDb.EntityIUserMaterial.AddRange(
            new EntityIUserMaterial { UserId = UserId, MaterialId = 1, Count = 5 },
            new EntityIUserMaterial { UserId = UserId, MaterialId = 2, Count = 7 });
        userDb.EntitySUserGift.Add(new EntitySUserGift { UserId = UserId, UserGiftUuid = "gift-1" });
        UserDataStore store = CreateStoreWithUser(UserId, userDb);
        DiffInterceptor diff = new(store, NullLogger<DiffInterceptor>.Instance);
        FakeServerCallContext context = ContextFor(UserId);

        await Journal().UnaryServerHandler(new SetUserNameRequest(), context, (request, ctx) =>
            diff.UnaryServerHandler(request, ctx, (_, _) =>
            {
                userDb.EntityIUserStatus[0].Level = 11;
                userDb.EntityIUserStatus[0].Exp = 0;
                userDb.EntityIUserMaterial[0].Count = 6;
                userDb.EntityIUserMaterial.RemoveAt(1);
                userDb.EntityIUserMaterial.Add(new EntityIUserMaterial { UserId = UserId, MaterialId = 3, Count = 1 });
                userDb.EntitySUserGift.Clear();
                return Task.FromResult(new SetUserNameResponse());
            }));

        string journal = File.ReadAllText(Path.Combine(_directory, "2026-10-24.log"));
        Assert.StartsWith($"2026-10-24 12:03:45.123 #{context.GetRequestId()} ", journal);
        Assert.Contains($" user {UserId}\n", journal);
        Assert.Contains("  IUserStatus\n    ~ level 10 → 11, exp 100 → 0\n", journal);
        Assert.Contains("  IUserMaterial\n    ~ materialId=1: count 5 → 6\n", journal);
        Assert.Contains("    + {\"materialId\":3,\"count\":1,", journal);
        Assert.Contains("    - {\"materialId\":2,\"count\":7,", journal);
        Assert.Contains("  SUserGift\n    - {\"userGiftUuid\":\"gift-1\"", journal);
        Assert.DoesNotContain("userId", journal);
    }

    [Fact]
    public async Task CallWithoutChanges_IsJournaledAsSuch_AndCallsWithoutAUserAreNot()
    {
        UserDataStore store = CreateStoreWithUser(UserId, new DarkUserMemoryDatabase { UserId = UserId });
        DiffInterceptor diff = new(store, NullLogger<DiffInterceptor>.Instance);

        await Journal().UnaryServerHandler(new SetUserNameRequest(), ContextFor(UserId), (request, ctx) =>
            diff.UnaryServerHandler(request, ctx, (_, _) => Task.FromResult(new SetUserNameResponse())));
        await Journal().UnaryServerHandler(new SetUserNameRequest(), FakeServerCallContext.Anonymous(), (request, ctx) =>
            diff.UnaryServerHandler(request, ctx, (_, _) => Task.FromResult(new SetUserNameResponse())));

        string[] lines = File.ReadAllLines(Path.Combine(_directory, "2026-10-24.log"));
        Assert.Equal(2, lines.Length);
        Assert.Equal("  (no changes)", lines[1]);
    }

    [Fact]
    public void ShortMethod_DropsThePackage()
        => Assert.Equal("CostumeService/Enhance", RequestJournalInterceptor.ShortMethod("/apb.api.costume.CostumeService/Enhance"));
}
