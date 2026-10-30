using MariesWonderland.Helpers;
using MariesWonderland.Proto.BigHunt;

namespace MariesWonderland.Tests.Helpers;

public class BigHuntRewardHelperTests
{
    [Fact]
    public void Combine_MergesSamePossessionIntoOneEntryWithSummedCount()
    {
        BigHuntReward original = new() { PossessionType = 1, PossessionId = 30, Count = 800 };

        List<BigHuntReward> combined = BigHuntRewardHelper.Combine(
        [
            original,
            new BigHuntReward { PossessionType = 1, PossessionId = 37, Count = 750 },
            new BigHuntReward { PossessionType = 1, PossessionId = 30, Count = 800 },
            new BigHuntReward { PossessionType = 2, PossessionId = 30, Count = 5 },
        ]);

        Assert.Collection(
            combined,
            r => Assert.Equal((1, 30, 1600), (r.PossessionType, r.PossessionId, r.Count)),
            r => Assert.Equal((1, 37, 750), (r.PossessionType, r.PossessionId, r.Count)),
            r => Assert.Equal((2, 30, 5), (r.PossessionType, r.PossessionId, r.Count)));
        Assert.Equal(800, original.Count);
    }
}
