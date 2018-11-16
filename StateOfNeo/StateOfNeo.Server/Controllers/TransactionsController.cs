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
using StateOfNeo.Common.Enums;

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
        public async Task<IActionResult> List(int page = 1, int pageSize = 10, string blockHash = null, string address = null, string asset = null)
        {
            if (!string.IsNullOrEmpty(address))
            {
                var res = this.transactions.TransactionsForAddress(address, page, pageSize);
                return this.Ok(res.ToListResult());
            }

            if (!string.IsNullOrEmpty(asset))
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var res = this.transactions.TransactionsForAsset(asset, page, pageSize);
                sw.Stop();
                return this.Ok(res.ToListResult());
            }

            var result = this.transactions.GetPageTransactions<TransactionListViewModel>(page, pageSize, blockHash);

            return this.Ok(result.ToListResult());
        }

        [HttpPost("[action]")]
        public IActionResult Chart([FromBody]ChartFilterViewModel filter)
        {
            var result = this.transactions.GetStats(filter);
            return this.Ok(result);
        }
        
        [HttpPost("[action]")]
        public IActionResult AddressChart([FromBody]ChartFilterViewModel filter, string address)
        {
            var result = this.transactions.GetTransactionsForAddressChart(filter, address);
            return this.Ok(result);
        }

        [HttpPost("[action]")]
        public IActionResult AssetChart([FromBody]ChartFilterViewModel filter, string assetHash)
        {
            var result = this.transactions.GetTransactionsForAssetChart(filter, assetHash);
            return this.Ok(result);
        } 

        [HttpGet("[action]")]
        public IActionResult TransactionTypesForAddress(string address)
        {
            var result = this.transactions.GetTransactionTypesForAddress(address);
            return this.Ok(result);
        }

        [HttpGet("[action]")]
        public IActionResult PieChart()
        {
            IEnumerable<ChartStatsViewModel> result = this.transactions.GetPieStats();
            return this.Ok(result);
        }

        [HttpGet("[action]")]
        public IActionResult AveragePer([FromQuery]UnitOfTime unit = UnitOfTime.Day)
        {
            return this.Ok(this.transactions.AveragePer(unit));
        }

        [HttpGet("[action]")]
        public IActionResult Total()
        {
            return this.Ok(this.transactions.Total());
        }

        [HttpGet("[action]")]
        public IActionResult TotalClaimed()
        {
            var result = this.transactions.TotalClaimed();
            return this.Ok(result);
        }
    }
}
