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

        [HttpGet("[action]")]
        public IActionResult GetAll()
        {
            return Ok(this.contracts.GetAll<SmartContractListViewModel>());
        }
    }
}
