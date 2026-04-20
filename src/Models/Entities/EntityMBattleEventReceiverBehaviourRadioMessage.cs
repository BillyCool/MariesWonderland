using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleEventReceiverBehaviourRadioMessage))]
public class EntityMBattleEventReceiverBehaviourRadioMessage
{
    [Key(0)] public int BattleEventReceiverBehaviourId { get; set; }

    [Key(1)] public int SpeakerId { get; set; }

    [Key(2)] public string ScenarioKey { get; set; }
}
