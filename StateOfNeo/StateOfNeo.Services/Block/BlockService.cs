using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StateOfNeo.Data;
using StateOfNeo.Data.Models;
using X.PagedList;

namespace StateOfNeo.Services.Block
{
    public class BlockService : IBlockService
    {
        private readonly StateOfNeoContext db;

        public BlockService(StateOfNeoContext db)
        {
            this.db = db;
        }

        public Data.Models.Block Find(string hash) =>
            this.db.Blocks
                .Where(x => x.Hash == hash)
                .FirstOrDefault();
    }
}
