using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Proto.Movie;

namespace MariesWonderland.Services;

public class MovieService(UserDataStore store) : MariesWonderland.Proto.Movie.MovieService.MovieServiceBase
{
    private readonly UserDataStore _store = store;

    /// <summary>Records one or more cutscenes as viewed, so the client can track which have been watched.</summary>
    public override Task<SaveViewedMovieResponse> SaveViewedMovie(SaveViewedMovieRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        foreach (int movieId in request.MovieId)
        {
            EntityIUserMovie? existing = userDb.EntityIUserMovie.FirstOrDefault(m => m.MovieId == movieId);
            if (existing != null)
            {
                existing.LatestViewedDatetime = nowMs;
            }
            else
            {
                userDb.EntityIUserMovie.Add(new EntityIUserMovie { UserId = userId, MovieId = movieId, LatestViewedDatetime = nowMs });
            }
        }

        return Task.FromResult(new SaveViewedMovieResponse());
    }
}
