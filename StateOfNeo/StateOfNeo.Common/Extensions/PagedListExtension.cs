using X.PagedList;

namespace StateOfNeo.Common.Extensions
{
    public static class PagedListExtension
    {
        public static object ToListResult<T>(this IPagedList<T> list)
        {
            return new { Items = list, MetaData = list.GetMetaData() };
        }
    }
}
