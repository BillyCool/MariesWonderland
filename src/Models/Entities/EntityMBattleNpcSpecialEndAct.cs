using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleNpcSpecialEndAct
{
    public int QuestSceneId { get; set; }

    public int WaveNumber { get; set; }

    public long BattleNpcId { get; set; }

    public string BattleNpcDeckCharacterUuid { get; set; }

    public SpecialEndBattleActType SpecialEndBattleActType { get; set; }
}
