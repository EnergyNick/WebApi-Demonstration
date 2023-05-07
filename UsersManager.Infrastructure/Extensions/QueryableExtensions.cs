namespace UsersManager.Infrastructure.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<TValue> PaginateIfNeeded<TValue>(this IQueryable<TValue> sources,
        int? size, int? index = null)
    {
        index ??= 0;
        return size == null
            ? sources
            : sources.Skip(size.Value * index.Value).Take(size.Value);
    }
}