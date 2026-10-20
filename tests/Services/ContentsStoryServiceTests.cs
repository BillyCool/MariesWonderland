using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Proto.ContentsStory;
using MariesWonderland.Tests.Infrastructure;
using ContentsStoryServiceImpl = MariesWonderland.Services.ContentsStoryService;

namespace MariesWonderland.Tests.Services;

public class ContentsStoryServiceTests(MasterDatabaseFixture fixture) : ServiceTestBase(fixture)
{
    private const long UserId = 1L;
    private const int FirstStoryId = 101;
    private const int SecondStoryId = 202;

    private static ContentsStoryServiceImpl CreateService(UserDataStore store) => new(store, TimeProvider.System);

    [Fact]
    public async Task RegisterPlayed_NewStory_AddsRecordWithCurrentTimestamp()
    {
        var userDb = CreateUserDb();
        var store = CreateStore(UserId, userDb);
        var service = CreateService(store);
        long beforeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        RegisterPlayedResponse response = await service.RegisterPlayed(
            new RegisterPlayedRequest { ContentsStoryId = FirstStoryId },
            ContextFor(UserId));

        long afterMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        Assert.NotNull(response);
        EntityIUserContentsStory record = Assert.Single(userDb.EntityIUserContentsStory);
        Assert.Equal(UserId, record.UserId);
        Assert.Equal(FirstStoryId, record.ContentsStoryId);
        Assert.InRange(record.PlayDatetime, beforeMs, afterMs);
    }

    [Fact]
    public async Task RegisterPlayed_ExistingStory_UpdatesTimestampWithoutDuplicatingRecord()
    {
        var userDb = CreateUserDb();
        userDb.EntityIUserContentsStory.AddRange(
        [
            new EntityIUserContentsStory
            {
                UserId = UserId,
                ContentsStoryId = FirstStoryId,
                PlayDatetime = 123
            },
            new EntityIUserContentsStory
            {
                UserId = UserId,
                ContentsStoryId = SecondStoryId,
                PlayDatetime = 456
            }
        ]);
        var store = CreateStore(UserId, userDb);
        var service = CreateService(store);
        long beforeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        await service.RegisterPlayed(
            new RegisterPlayedRequest { ContentsStoryId = FirstStoryId },
            ContextFor(UserId));

        long afterMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        Assert.Equal(2, userDb.EntityIUserContentsStory.Count);

        EntityIUserContentsStory updatedRecord = Assert.Single(
            userDb.EntityIUserContentsStory, record => record.ContentsStoryId == FirstStoryId);
        Assert.InRange(updatedRecord.PlayDatetime, beforeMs, afterMs);

        EntityIUserContentsStory untouchedRecord = Assert.Single(
            userDb.EntityIUserContentsStory, record => record.ContentsStoryId == SecondStoryId);
        Assert.Equal(456, untouchedRecord.PlayDatetime);
    }

    [Fact]
    public async Task RegisterPlayed_MissingUserDatabase_CreatesUserDatabaseAndPersistsRecord()
    {
        UserDataStore store = new(TimeProvider.System);
        var service = CreateService(store);

        await service.RegisterPlayed(
            new RegisterPlayedRequest { ContentsStoryId = FirstStoryId },
            ContextFor(UserId));

        Assert.True(store.TryGet(UserId, out DarkUserMemoryDatabase? userDb));
        Assert.NotNull(userDb);

        EntityIUserContentsStory record = Assert.Single(userDb.EntityIUserContentsStory);
        Assert.Equal(UserId, record.UserId);
        Assert.Equal(FirstStoryId, record.ContentsStoryId);
        Assert.True(record.PlayDatetime > 0);
    }
}
