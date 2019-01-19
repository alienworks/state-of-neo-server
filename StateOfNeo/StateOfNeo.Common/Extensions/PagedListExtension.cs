using X.PagedList;

namespace StateOfNeo.Common.Extensions
{
    public static class PagedListExtension
    {
        public static PagedListResult<T> ToListResult<T>(this IPagedList<T> list)
        {
            return new PagedListResult<T> { Items = list, MetaData = list.GetMetaData() };
        }
    }
}
