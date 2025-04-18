﻿namespace PSOServerWebsite;

public static class IEnumerableExtensions
{
    internal static IEnumerable<TResult> FullOuterJoin<TA, TB, TKey, TResult>(
            this IEnumerable<TA> a,
            IEnumerable<TB> b,
            Func<TA, TKey> selectKeyA,
            Func<TB, TKey> selectKeyB,
            Func<TA, TB, TKey, TResult> projection,
            TA? defaultA = default,
            TB? defaultB = default,
            IEqualityComparer<TKey>? cmp = null)
    {
        cmp ??= EqualityComparer<TKey>.Default;
        ILookup<TKey, TA> alookup = a.ToLookup(selectKeyA, cmp);
        ILookup<TKey, TB> blookup = b.ToLookup(selectKeyB, cmp);

        var keys = new HashSet<TKey>(alookup.Select(p => p.Key), cmp);
        keys.UnionWith(blookup.Select(p => p.Key));

        IEnumerable<TResult> join = from key in keys
                   from xa in alookup[key].DefaultIfEmpty(defaultA)
                   from xb in blookup[key].DefaultIfEmpty(defaultB)
                   select projection(xa, xb, key);

        return join;
    }
}
