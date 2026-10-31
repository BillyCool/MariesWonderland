using Grpc.Core;
using MariesWonderland.Helpers;

namespace MariesWonderland.Tests.Helpers;

public class GameErrorTests
{
    [Fact]
    public void Refuse_SendsTheClientsErrorTrailers()
    {
        RpcException error = GameError.Refuse("Your friend list is full", NetworkCode.FriendLimit);

        Assert.Equal(StatusCode.FailedPrecondition, error.StatusCode);
        Assert.Equal("1", error.Trailers.GetValue("x-apb-screen-transition-type"));
        Assert.Equal("204002", error.Trailers.GetValue("x-apb-message-code"));
        Assert.Equal("204002", error.Trailers.GetValue("x-apb-message-text-id"));
    }

    [Fact]
    public void Refuse_DefaultsToTheGenericProcessFailedText()
    {
        RpcException error = GameError.Refuse("whatever", transition: ScreenTransition.FunctionTop);

        Assert.Equal("2", error.Trailers.GetValue("x-apb-screen-transition-type"));
        Assert.Equal("900002", error.Trailers.GetValue("x-apb-message-code"));
    }
}
