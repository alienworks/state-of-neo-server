using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StateOfNeo.Services.Transaction;
using StateOfNeo.Services;
using StateOfNeo.ViewModels.Transaction;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using StateOfNeo.Common.Extensions;
using StateOfNeo.Data.Models.Transactions;
using StateOfNeo.ViewModels.Chart;

namespace StateOfNeo.Server.Controllers
{
    public class TransactionsController : BaseApiController
    {
        private readonly IPaginatingService paginating;
        private readonly ITransactionService transactions;

        public TransactionsController(IPaginatingService paginating, ITransactionService transactions)
        {
            this.paginating = paginating;
            this.transactions = transactions;
        }
               
        [HttpGet("[action]/{hash}")]
        public IActionResult Get(string hash)
        {
            var transaction = this.transactions.Find<TransactionDetailsViewModel>(hash);
            if (transaction == null)
            {
                return this.BadRequest("Invalid block hash");
            }

            return this.Ok(transaction);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> List(int page = 1, int pageSize = 10, string blockHash = null)
        {
            var result = await this.paginating.GetPage<Transaction, TransactionListViewModel>(
                page, 
                pageSize, 
                x => x.Block.Timestamp.ToUnixDate(),
                x => blockHash == null ? true : x.Block.Hash == blockHash);

            return this.Ok(result.ToListResult());
        }

        [HttpGet("[action]")]
        public IActionResult TotalClaimed()
        {
            var result = this.transactions.TotalClaimed();
            return this.Ok(result);
        }

        [HttpPost("[action]")]
        public IActionResult Chart([FromBody]ChartFilterViewModel filter)
        {
            var result = this.transactions.GetStats(filter);
            return this.Ok(result);
        }
    }
}
