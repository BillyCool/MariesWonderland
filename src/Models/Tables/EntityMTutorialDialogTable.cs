using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMTutorialDialogTable : TableBase<EntityMTutorialDialog>
{
    private readonly Func<EntityMTutorialDialog, TutorialType> primaryIndexSelector;

    public EntityMTutorialDialogTable(EntityMTutorialDialog[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.TutorialType;
    }

    public EntityMTutorialDialog FindByTutorialType(TutorialType key) => FindUniqueCore(data, primaryIndexSelector, Comparer<TutorialType>.Default, key);
}
