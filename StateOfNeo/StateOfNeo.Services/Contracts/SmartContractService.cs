using AutoMapper.QueryableExtensions;
using StateOfNeo.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateOfNeo.Services
{
    public class SmartContractService : ISmartContractService
    {
        private readonly StateOfNeoContext db;

        public SmartContractService(StateOfNeoContext db)
        {
            this.db = db;
        }

        public IEnumerable<T> GetAll<T>() => this.db.SmartContracts.ProjectTo<T>();
    }
}
