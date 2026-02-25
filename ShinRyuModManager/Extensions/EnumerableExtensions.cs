namespace ShinRyuModManager.Extensions;

public static class EnumerableExtensions
{
    public static bool IsNullOrEmpty<TSource>(this IEnumerable<TSource> collection)
    {
        return collection == null || !collection.Any();
    }
}
