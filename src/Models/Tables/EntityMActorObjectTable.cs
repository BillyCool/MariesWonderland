using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMActorObjectTable : TableBase<EntityMActorObject>
{
    private readonly Func<EntityMActorObject, int> primaryIndexSelector;

    public EntityMActorObjectTable(EntityMActorObject[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.ActorObjectId;
    }

    public EntityMActorObject FindByActorObjectId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
