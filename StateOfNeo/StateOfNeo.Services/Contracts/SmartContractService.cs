using AutoMapper.QueryableExtensions;
using StateOfNeo.Data;
using StateOfNeo.ViewModels.Contracts;
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

        public SmartContractDetailsViewModel Find(string hash) =>
            this.db.SmartContracts
                .Where(x => x.Hash == hash)
                .ProjectTo<SmartContractDetailsViewModel>()
                .FirstOrDefault();

        public IEnumerable<T> GetAll<T>() => this.db.SmartContracts.ProjectTo<T>();
    }
}
