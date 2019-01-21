using Microsoft.AspNetCore.Mvc;
using StateOfNeo.Services;
using StateOfNeo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
