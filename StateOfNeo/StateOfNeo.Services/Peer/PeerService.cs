using AutoMapper.QueryableExtensions;
using StateOfNeo.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateOfNeo.Services
{
    public class PeerService : IPeerService
    {
        private readonly StateOfNeoContext db;

        public PeerService(StateOfNeoContext db)
        {
            this.db = db;
        }

        public IEnumerable<T> GetAll<T>() =>
            this.db.Peers
                .Where(x => x.Longitude.HasValue && x.Latitude.HasValue)
                .ProjectTo<T>();
    }
}
