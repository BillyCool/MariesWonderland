using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMActorAnimationControllerTable : TableBase<EntityMActorAnimationController>
{
    private readonly Func<EntityMActorAnimationController, int> primaryIndexSelector;

    public EntityMActorAnimationControllerTable(EntityMActorAnimationController[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.ActorAnimationControllerId;
    }
}
