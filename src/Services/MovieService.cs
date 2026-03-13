using MariesWonderland.Proto.Movie;
using Grpc.Core;

namespace MariesWonderland.Services;

public class MovieService : MariesWonderland.Proto.Movie.MovieService.MovieServiceBase
{
    public override Task<SaveViewedMovieResponse> SaveViewedMovie(SaveViewedMovieRequest request, ServerCallContext context)
    {
        return Task.FromResult(new SaveViewedMovieResponse());
    }
}
