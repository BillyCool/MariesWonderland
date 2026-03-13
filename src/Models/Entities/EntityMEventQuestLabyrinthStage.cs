using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMEventQuestLabyrinthStage
{
    public int EventQuestChapterId { get; set; }

    public int StageOrder { get; set; }

    public int StartSequenceSortOrder { get; set; }

    public int EndSequenceSortOrder { get; set; }

    public int StageClearRewardGroupId { get; set; }

    public int StageAccumulationRewardGroupId { get; set; }

    public int Mob1Id { get; set; }

    public int Mob2Id { get; set; }

    public int TreasureAssetId { get; set; }
}
