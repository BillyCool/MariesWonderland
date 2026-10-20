using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Proto.Movie;
using MariesWonderland.Tests.Infrastructure;
using MovieServiceImpl = MariesWonderland.Services.MovieService;

namespace MariesWonderland.Tests.Services;

public class MovieServiceTests(MasterDatabaseFixture fixture) : ServiceTestBase(fixture)
{
    private const long UserId = 1L;
    private const int FirstMovieId = 101;
    private const int SecondMovieId = 202;

    private static MovieServiceImpl CreateService(UserDataStore store) => new(store, TimeProvider.System);

    [Fact]
    public async Task SaveViewedMovie_NewMovies_AddsRecordsWithCurrentTimestamp()
    {
        var userDb = CreateUserDb();
        var store = CreateStore(UserId, userDb);
        var service = CreateService(store);
        long beforeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        SaveViewedMovieResponse response = await service.SaveViewedMovie(
            new SaveViewedMovieRequest { MovieId = { FirstMovieId, SecondMovieId } },
            ContextFor(UserId));

        long afterMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        Assert.NotNull(response);
        Assert.Equal(2, userDb.EntityIUserMovie.Count);

        EntityIUserMovie firstRecord = Assert.Single(userDb.EntityIUserMovie, movie => movie.MovieId == FirstMovieId);
        Assert.Equal(UserId, firstRecord.UserId);
        Assert.InRange(firstRecord.LatestViewedDatetime, beforeMs, afterMs);

        EntityIUserMovie secondRecord = Assert.Single(userDb.EntityIUserMovie, movie => movie.MovieId == SecondMovieId);
        Assert.Equal(UserId, secondRecord.UserId);
        Assert.InRange(secondRecord.LatestViewedDatetime, beforeMs, afterMs);
    }

    [Fact]
    public async Task SaveViewedMovie_ExistingMovie_UpdatesTimestampWithoutDuplicatingRecord()
    {
        var userDb = CreateUserDb();
        userDb.EntityIUserMovie.AddRange(
        [
            new EntityIUserMovie
            {
                UserId = UserId,
                MovieId = FirstMovieId,
                LatestViewedDatetime = 123
            },
            new EntityIUserMovie
            {
                UserId = UserId,
                MovieId = SecondMovieId,
                LatestViewedDatetime = 456
            }
        ]);
        var store = CreateStore(UserId, userDb);
        var service = CreateService(store);
        long beforeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        await service.SaveViewedMovie(
            new SaveViewedMovieRequest { MovieId = { FirstMovieId } },
            ContextFor(UserId));

        long afterMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        Assert.Equal(2, userDb.EntityIUserMovie.Count);

        EntityIUserMovie updatedRecord = Assert.Single(
            userDb.EntityIUserMovie,
            movie => movie.MovieId == FirstMovieId);
        Assert.InRange(updatedRecord.LatestViewedDatetime, beforeMs, afterMs);

        EntityIUserMovie untouchedRecord = Assert.Single(
            userDb.EntityIUserMovie,
            movie => movie.MovieId == SecondMovieId);
        Assert.Equal(456, untouchedRecord.LatestViewedDatetime);
    }

    [Fact]
    public async Task SaveViewedMovie_DuplicateMovieIdsInRequest_DoesNotCreateDuplicateRows()
    {
        var userDb = CreateUserDb();
        var store = CreateStore(UserId, userDb);
        var service = CreateService(store);

        await service.SaveViewedMovie(
            new SaveViewedMovieRequest { MovieId = { FirstMovieId, FirstMovieId, SecondMovieId } },
            ContextFor(UserId));

        Assert.Equal(2, userDb.EntityIUserMovie.Count);
        Assert.Single(userDb.EntityIUserMovie, movie => movie.MovieId == FirstMovieId);
        Assert.Single(userDb.EntityIUserMovie, movie => movie.MovieId == SecondMovieId);
    }

    [Fact]
    public async Task SaveViewedMovie_MissingUserDatabase_CreatesUserDatabaseAndPersistsRecords()
    {
        UserDataStore store = new(TimeProvider.System);
        var service = CreateService(store);

        await service.SaveViewedMovie(
            new SaveViewedMovieRequest { MovieId = { FirstMovieId } },
            ContextFor(UserId));

        Assert.True(store.TryGet(UserId, out DarkUserMemoryDatabase? userDb));
        Assert.NotNull(userDb);
        Assert.Equal(UserId, userDb.UserId);

        EntityIUserMovie record = Assert.Single(userDb.EntityIUserMovie);
        Assert.Equal(UserId, record.UserId);
        Assert.Equal(FirstMovieId, record.MovieId);
        Assert.True(record.LatestViewedDatetime > 0);
    }

    [Fact]
    public async Task SaveViewedMovie_EmptyRequest_DoesNotMutateExistingRows()
    {
        var userDb = CreateUserDb();
        userDb.EntityIUserMovie.Add(new EntityIUserMovie
        {
            UserId = UserId,
            MovieId = FirstMovieId,
            LatestViewedDatetime = 789
        });
        var store = CreateStore(UserId, userDb);
        var service = CreateService(store);

        SaveViewedMovieResponse response = await service.SaveViewedMovie(
            new SaveViewedMovieRequest(),
            ContextFor(UserId));

        Assert.NotNull(response);
        EntityIUserMovie record = Assert.Single(userDb.EntityIUserMovie);
        Assert.Equal(FirstMovieId, record.MovieId);
        Assert.Equal(789, record.LatestViewedDatetime);
    }
}
