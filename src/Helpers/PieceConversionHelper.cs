using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Helpers;

/// <summary>
/// Piece materials that convert on their own: every time a player holds enough of one, the pieces turn into the
/// finished item. The server records each conversion in <c>IUserPossessionAutoConvert</c>; the client watches that
/// table and opens the "Automatic Conversion" dialog for the rows that change.
/// </summary>
public static class PieceConversionHelper
{
    /// <summary>One piece material and what <paramref name="FromCount"/> of it turn into.</summary>
    public sealed record Rule(int MaterialId, int FromCount, int ToMaterialId, int ToCount);

    // REVIEW: the rules aren't in master data. These two are the conversions seen in the real save's
    // IUserPossessionAutoConvert (10 pieces into 1). Other pieces may convert too. They belong in a server-side
    // table seeded at startup, not in code (see the roadmap's server-side rule tables).
    private static readonly Rule[] Rules =
    [
        new(501001, 10, 322002, 1),
        new(501002, 10, 312003, 1),
    ];

    public static Rule? FindRule(int materialId) => Array.Find(Rules, r => r.MaterialId == materialId);

    /// <summary>
    /// Converts a piece material the player now holds enough of: takes the pieces, grants the finished items and
    /// writes the conversion row. Does nothing for other materials or when there are too few pieces.
    /// </summary>
    public static void ConvertIfEnough(DarkUserMemoryDatabase userDb, int materialId, long nowMs)
    {
        Rule? rule = FindRule(materialId);
        EntityIUserMaterial? pieces = userDb.EntityIUserMaterial.FirstOrDefault(m => m.MaterialId == materialId);
        if (rule == null || pieces == null || pieces.Count < rule.FromCount)
        {
            return;
        }

        int times = pieces.Count / rule.FromCount;
        pieces.Count -= times * rule.FromCount;

        EntityIUserMaterial? target = userDb.EntityIUserMaterial.FirstOrDefault(m => m.MaterialId == rule.ToMaterialId);
        if (target == null)
        {
            userDb.EntityIUserMaterial.Add(new EntityIUserMaterial { UserId = userDb.UserId, MaterialId = rule.ToMaterialId, Count = times * rule.ToCount, FirstAcquisitionDatetime = nowMs });
        }
        else
        {
            target.Count += times * rule.ToCount;
        }

        EntityIUserPossessionAutoConvert? row = userDb.EntityIUserPossessionAutoConvert
            .FirstOrDefault(c => c.PossessionType == PossessionType.MATERIAL && c.PossessionId == materialId);
        if (row == null)
        {
            row = new EntityIUserPossessionAutoConvert { UserId = userDb.UserId, PossessionType = PossessionType.MATERIAL, PossessionId = materialId };
            userDb.EntityIUserPossessionAutoConvert.Add(row);
        }

        row.FromCount = times * rule.FromCount;
        row.ToPossessionType = PossessionType.MATERIAL;
        row.ToPossessionId = rule.ToMaterialId;
        row.ToCount = times * rule.ToCount;
        row.ConvertDatetime = nowMs;
    }
}
