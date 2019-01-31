using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using StateOfNeo.Common.Extensions;
using StateOfNeo.Data;
using StateOfNeo.ViewModels.Chart;
using StateOfNeo.ViewModels.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using X.PagedList;

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

        public IPagedList<T> GetTransactions<T>(string hash, int page, int pageSize) =>
            this.db.InvocationTransactions
                .Include(x => x.Transaction)
                .Where(x => x.ContractHash == hash)
                .Where(x => x.TransactionHash != null)
                .OrderByDescending(x => x.Transaction.Timestamp)
                .ProjectTo<T>()
                .ToPagedList(page, pageSize);

        public IEnumerable<ChartStatsViewModel> ContractInvocationsChart(int count)
        {
            var result = this.db.SmartContracts
                .Select(x => new ChartStatsViewModel
                {
                    Label = x.Name,
                    Value = x.InvocationTransactions.Count()
                })
                .OrderByDescending(x => x.Value)
                .Take(count)
                .ToList();

            return result;
        }

        public int CreatedTotal() => this.db.SmartContracts.Count();

        public int CreatedLastMonth() => this.db.SmartContracts
            .Count(x => x.Timestamp.ToUnixDate() >= DateTime.UtcNow.AddMonths(-1));
    }
}
