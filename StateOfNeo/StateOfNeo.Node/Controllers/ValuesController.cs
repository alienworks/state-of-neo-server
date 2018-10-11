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

        [HttpGet("[action]")]
        public async Task<IActionResult> Post([FromQuery]string ip)
        {
            var remoteNodesCached = LocalNode.Singleton.GetRemoteNodes().ToList();
            var endPoint = new IPEndPoint(IPAddress.Parse(ip), 10333);
            var remoteConnect = new Connect(endPoint);

            Startup.NeoSystem.LocalNode.Tell(remoteConnect, Startup.NeoSystem.LocalNode);

            var remoteNodes = LocalNode.Singleton.GetRemoteNodes();
            var success = remoteNodes.Any(rn => rn.Remote.Address.ToString().ToMatchedIp() == ip);

            return this.Ok(success);
        }
    }
}
