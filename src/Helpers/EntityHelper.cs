using MariesWonderland.Models.Entities;

namespace MariesWonderland.Helpers;

/// <summary>
/// Extension methods on <see cref="List{T}"/> for adding and retrieving user entities,
/// supporting both predicate-based and <see cref="IUserEntity.UserId"/>-based lookups with lazy creation.
/// </summary>
public static class EntityHelper
{
    /// <summary>Adds an entity to a list and returns it (convenience for inline new-entity seeding).</summary>
    public static T AddNew<T>(this List<T> list, T entity)
    {
        list.Add(entity);
        return entity;
    }

    /// <summary>Returns the first element matching <paramref name="predicate"/>, creating and adding one via <paramref name="factory"/> if none match.</summary>
    public static T GetOrCreate<T>(this List<T> list, Func<T, bool> predicate, Func<T> factory)
    {
        T? existing = list.FirstOrDefault(predicate);
        if (existing is not null) return existing;
        T entity = factory();
        list.Add(entity);
        return entity;
    }

    /// <summary>Returns the first element matching by <see cref="IUserEntity.UserId"/>, creating and adding one via <paramref name="factory"/> if none match.</summary>
    public static T GetOrCreate<T>(this List<T> list, long userId, Func<T>? factory = null)
        where T : IUserEntity, new()
    {
        T? existing = list.FirstOrDefault(e => e.UserId == userId);
        if (existing is not null) return existing;
        T entity = factory is not null ? factory() : new T { UserId = userId };
        list.Add(entity);
        return entity;
    }
}
