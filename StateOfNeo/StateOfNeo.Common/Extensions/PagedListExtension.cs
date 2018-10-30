using X.PagedList;

namespace StateOfNeo.Common.Extensions
{
    public static class PagedListExtension
    {
        public static object ToObjectResult<T>(this IPagedList<T> list)
        {
            return new { list, meta = list.GetMetaData() };
        }
    }
}
