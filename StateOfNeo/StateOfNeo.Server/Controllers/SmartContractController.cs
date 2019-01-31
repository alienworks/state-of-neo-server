﻿using Microsoft.AspNetCore.Mvc;
using StateOfNeo.Common.Extensions;
using StateOfNeo.Services;
using StateOfNeo.ViewModels;
using StateOfNeo.ViewModels.Transaction;

namespace StateOfNeo.Server.Controllers
{
    public class SmartContractController : BaseApiController
    {
        private readonly ISmartContractService contracts;

        public SmartContractController(ISmartContractService contracts)
        {
            this.contracts = contracts;
        }

        [HttpGet("[action]/{hash}")]
        public IActionResult Get(string hash)
        {
            var result = this.contracts.Find(hash);
            if (result == null)
            {
                return this.NotFound();
            }

            return this.Ok(result);
        }

        [HttpGet("[action]")]
        public IActionResult GetAll()
        {
            var result = this.contracts.GetAll<SmartContractListViewModel>();
            return this.Ok(result);
        }


        [HttpGet("[action]/{hash}/{page}/{pageSize}")]
        public IActionResult InvocationTransactions(
            string hash,
            int page = 1,
            int pageSize = 10)
        {
            var result = this.contracts.GetTransactions<TransactionListViewModel>(hash, page, pageSize);

            return Ok(result.ToListResult());
        }
    }
}
