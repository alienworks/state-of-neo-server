using System.Linq;
using StateOfNeo.Data;
using StateOfNeo.Data.Models;

namespace StateOfNeo.Services
{
    public class AssetService : IAssetService
    {
        private readonly StateOfNeoContext db;

        public AssetService(StateOfNeoContext db)
        {
            this.db = db;
        }

        public Asset Find(string hash) => 
            this.db.Assets
                .Where(x => x.Hash.ToString() == hash)
                .FirstOrDefault();        
    }
}
