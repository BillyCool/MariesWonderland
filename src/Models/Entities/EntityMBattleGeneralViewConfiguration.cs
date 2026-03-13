using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleGeneralViewConfiguration
{
    public int QuestSceneId { get; set; }

    public int WaveNumber { get; set; }

    public bool IsDisableBattleStartVoice { get; set; }

    public bool IsEnableWhiteFadeout { get; set; }

    public int EnvSeId { get; set; }

    public int WaveWinSeId { get; set; }

    public bool IsDisablePlayWinTimeline { get; set; }
}
