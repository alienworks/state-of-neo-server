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
        Data.Models.Block Find(string hash);
    }
}
