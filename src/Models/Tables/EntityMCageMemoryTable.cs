using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCageMemoryTable : TableBase<EntityMCageMemory>
{
    private readonly Func<EntityMCageMemory, int> primaryIndexSelector;
    private readonly Func<EntityMCageMemory, int> secondaryIndexSelector;

    public EntityMCageMemoryTable(EntityMCageMemory[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.CageMemoryId;
        secondaryIndexSelector = element => element.MainQuestSeasonId;
    }

    public EntityMCageMemory FindByCageMemoryId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
