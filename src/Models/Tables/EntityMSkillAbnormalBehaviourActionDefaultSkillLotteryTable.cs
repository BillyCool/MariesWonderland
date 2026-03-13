using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillAbnormalBehaviourActionDefaultSkillLotteryTable : TableBase<EntityMSkillAbnormalBehaviourActionDefaultSkillLottery>
{
    private readonly Func<EntityMSkillAbnormalBehaviourActionDefaultSkillLottery, int> primaryIndexSelector;

    public EntityMSkillAbnormalBehaviourActionDefaultSkillLotteryTable(EntityMSkillAbnormalBehaviourActionDefaultSkillLottery[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.SkillAbnormalBehaviourActionId;
    }

    public EntityMSkillAbnormalBehaviourActionDefaultSkillLottery FindBySkillAbnormalBehaviourActionId(int key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
