using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMActorTable : TableBase<EntityMActor>
{
    private readonly Func<EntityMActor, int> primaryIndexSelector;

    public EntityMActorTable(EntityMActor[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.ActorId;
    }

    public EntityMActor FindByActorId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);

    public bool TryFindByActorId(int key, out EntityMActor result) => TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);
}
