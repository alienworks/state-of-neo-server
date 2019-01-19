using System;
using System.Collections.Generic;
using System.Text;
using X.PagedList;

namespace StateOfNeo.Common
{
    public class PagedListResult<T>
    {
        public IPagedList<T> Items { get; set; }

        public PagedListMetaData MetaData { get; set; }
    }
}
