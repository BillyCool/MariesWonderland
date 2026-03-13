using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleQuestSceneBgm
{
    public int QuestSceneId { get; set; }

    public int StartWaveNumber { get; set; }

    public int BgmId { get; set; }

    public int Stem { get; set; }

    public int TrackNumber { get; set; }
}
