using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Neo.Ledger;
using Neo.Network.P2P;
using StateOfNeo.Common;
using StateOfNeo.Data;
using StateOfNeo.Server.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static Akka.IO.Tcp;

namespace StateOfNeo.Server.Controllers
{
    public class ValuesController : BaseApiController
    {
        private readonly StateOfNeoContext ctx;
        private readonly NodeSynchronizer nodeSynchronizer;
        private readonly LocationCaller locationCaller;
        private readonly NetSettings netSettings;

        public ValuesController(
            NodeSynchronizer nodeSynchronizer, 
            LocationCaller locationCaller, 
            StateOfNeoContext ctx, 
            IOptions<NetSettings> netSettings)
        {
            this.ctx = ctx;
            this.nodeSynchronizer = nodeSynchronizer;
            this.locationCaller = locationCaller;
            this.netSettings = netSettings.Value;
        }
        
        [HttpGet]
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
