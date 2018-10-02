using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Neo;
using Neo.Core;
using StateOfNeo.Common;
using StateOfNeo.Data;
using StateOfNeo.Data.Models;
using StateOfNeo.Data.Seed;
using StateOfNeo.Server.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Controllers
{
    public class ValuesController : BaseApiController
    {
        private readonly StateOfNeoContext _ctx;
        private readonly NodeSynchronizer _nodeSynchronizer;
        private readonly LocationCaller _locationCaller;
        private readonly NetSettings _netSettings;

        public ValuesController(NodeSynchronizer nodeSynchronizer, LocationCaller locationCaller, StateOfNeoContext ctx, IOptions<NetSettings> netSettings)
        {
            _ctx = ctx;
            _nodeSynchronizer = nodeSynchronizer;
            _locationCaller = locationCaller;
            _netSettings = netSettings.Value;
        }
        
        [HttpGet]
        public async Task<IActionResult> Post([FromQuery]string ip)
        {
            var remoteNodesCached = Startup.localNode.GetRemoteNodes().ToList();
            var endPoint = new IPEndPoint(IPAddress.Parse(ip), 10333);
            await Startup.localNode.ConnectToPeerAsync(endPoint);
            var remoteNodes = Startup.localNode.GetRemoteNodes();
            var success = remoteNodes.Any(rn => rn.RemoteEndpoint.Address.ToString().ToMatchedIp() == ip);

            return this.Ok(success);
        }
    }
}
