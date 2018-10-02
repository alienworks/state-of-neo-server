using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
            var a = LocalNode.Singleton;
            var b = object.ReferenceEquals(a, Startup.NeoSystem.LocalNode);

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
