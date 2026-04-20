using System.Collections;

namespace MariesWonderland.Models.Tables;

public readonly struct RangeView<T> : IReadOnlyList<T>
{
    public static RangeView<T> Empty;

    private readonly T[] OrderedData;

    private readonly int Left;

    private readonly int Right;

    private readonly bool Ascendant;

    private readonly bool HasValue;

    public int Count => HasValue ? Right + 1 - Left : 0;

    public T this[int index]
    {
        get
        {
            index = Ascendant ? Left + index : Right - index;
            return OrderedData[index];
        }
    }

    public RangeView(T[] orderedData, int left, int right, bool ascendant)
    {
        OrderedData = orderedData;
        Left = left;
        Right = right;
        Ascendant = ascendant;
        HasValue = left <= right && orderedData.Length != 0;
    }

    public IEnumerator<T> GetEnumerator()
    {
        var enumeration = OrderedData.Skip(Left).Take(Count);
        if (!Ascendant)
        {
            enumeration = enumeration.Reverse();
        }

        return enumeration.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
