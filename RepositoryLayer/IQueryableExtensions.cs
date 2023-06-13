using System;
using System.Linq;

namespace RepositoryLayer
{
    public static class LeftJoinExtension
    {
        public static IQueryable<TResult> LeftJoin<TSource, TInner, TKey, TResult>(this IQueryable<TSource> source, IQueryable<TInner> other, Func<TSource, TKey> func, Func<TInner, TKey> innerkey, Func<TSource, TInner, TResult> res)
        {
            return from f in source
                   join b in other on func.Invoke(f) equals innerkey.Invoke(b) into g
                   from result in g.DefaultIfEmpty()
                   select res.Invoke(f, result);
        }
    }
}
