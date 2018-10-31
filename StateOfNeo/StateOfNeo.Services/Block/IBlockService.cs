using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;

namespace StateOfNeo.Services.Block
{
    public interface IBlockService
    {
        T Find<T>(string hash);
        T Find<T>(int height);
    }
}
