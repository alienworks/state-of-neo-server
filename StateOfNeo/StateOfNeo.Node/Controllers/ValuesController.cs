using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Neo.Ledger;
using Neo.Network.P2P;
using StateOfNeo.Common;
using static Akka.IO.Tcp;

namespace StateOfNeo.Node.Controllers
{
    public class ValuesController : BaseApiController
    {
        // GET api/values
        [HttpGet("[action]")]
        public ActionResult<IActionResult> GetHeight()
        {
            var height = Blockchain.Singleton.Height.ToString();
            return Ok(height);
        }
    }
}
