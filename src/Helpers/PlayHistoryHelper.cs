using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Helpers;

/// <summary>The server-kept play-history counters (<see cref="EntitySPlayHistory"/>).</summary>
public static class PlayHistoryHelper
{
    public static void Add(DarkUserMemoryDatabase userDb, GamePlayHistoryType type, long amount)
    {
        EntitySPlayHistory row = userDb.EntitySPlayHistory.GetOrCreate(
            h => h.GamePlayHistoryType == type,
            () => new EntitySPlayHistory { UserId = userDb.UserId, GamePlayHistoryType = type });
        row.Count += amount;
    }

    /// <summary>The counter's value; 0 when nothing was recorded.</summary>
    public static long Get(DarkUserMemoryDatabase userDb, GamePlayHistoryType type)
        => userDb.EntitySPlayHistory.FirstOrDefault(h => h.GamePlayHistoryType == type)?.Count ?? 0;
}
