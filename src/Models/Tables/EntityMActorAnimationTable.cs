using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMActorAnimationTable : TableBase<EntityMActorAnimation>
{
    private readonly Func<EntityMActorAnimation, int> primaryIndexSelector;

    public EntityMActorAnimationTable(EntityMActorAnimation[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.ActorAnimationId;
    }

    public EntityMActorAnimation FindByActorAnimationId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
