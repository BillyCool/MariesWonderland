using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMPvpSeasonGrade
{
    public int PvpGradeId { get; set; }

    public int PvpSeasonId { get; set; }

    public int NecessaryPvpPoint { get; set; }

    public int IconAssetId { get; set; }

    public int PvpGradeWeeklyRewardGroupId { get; set; }

    public int PvpGradeOneMatchRewardGroupId { get; set; }
}
