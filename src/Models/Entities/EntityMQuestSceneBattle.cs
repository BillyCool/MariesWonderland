using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMQuestSceneBattle
{
    public int QuestSceneId { get; set; }

    public int BattleGroupId { get; set; }

    public int BattleDropBoxGroupId { get; set; }

    public int BattleFieldLocaleSettingIndex { get; set; }

    public int BattleEventGroupId { get; set; }

    public int PostProcessConfigurationIndex { get; set; }
}
