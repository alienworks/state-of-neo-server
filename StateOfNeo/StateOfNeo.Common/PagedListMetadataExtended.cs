using System;
using System.Collections.Generic;
using System.Text;
using X.PagedList;

namespace StateOfNeo.Common
{
    public class PagedListMetadataExtended : PagedListMetaData
    {
        public static PagedListMetadataExtended FromParent(PagedListMetaData parent)
        {
            return new PagedListMetadataExtended
            {
                FirstItemOnPage = parent.FirstItemOnPage,
                HasNextPage = parent.HasNextPage,
                HasPreviousPage = parent.HasPreviousPage,
                IsFirstPage = parent.IsFirstPage,
                IsLastPage = parent.IsLastPage,
                LastItemOnPage = parent.LastItemOnPage,
                PageCount = parent.PageCount,
                PageNumber = parent.PageNumber,
                PageSize = parent.PageSize,
                TotalItemCount = parent.TotalItemCount
            };
        }

        public new int PageCount { get; set; }

        public new int TotalItemCount { get; set; }
    }
}
