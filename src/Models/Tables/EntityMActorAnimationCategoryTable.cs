using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMActorAnimationCategoryTable : TableBase<EntityMActorAnimationCategory>
{
    private readonly Func<EntityMActorAnimationCategory, int> primaryIndexSelector;

    public EntityMActorAnimationCategoryTable(EntityMActorAnimationCategory[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.ActorAnimationCategoryId;
    }
}
