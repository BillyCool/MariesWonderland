using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserLoginTable : TableBase<EntityIUserLogin>
{
    private readonly Func<EntityIUserLogin, long> primaryIndexSelector;

    public EntityIUserLoginTable(EntityIUserLogin[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.UserId;
    }
}
