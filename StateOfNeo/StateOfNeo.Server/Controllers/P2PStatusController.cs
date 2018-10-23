using Microsoft.AspNetCore.Mvc;
using Neo.Network.P2P;
using StateOfNeo.Common;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static Akka.IO.Tcp;

namespace StateOfNeo.Server.Controllers
{
    public class P2PStatusController: BaseApiController
    {
        [HttpPost("[action]/{ip}")]
        public async Task<IActionResult> Checkip(string ip)
        {
            var remoteNodesCached = LocalNode.Singleton.GetRemoteNodes().ToList();
            var endPoint = new IPEndPoint(IPAddress.Parse(ip), 10333);
            var remoteConnect = new Connect(endPoint);

            Program.NeoSystem.LocalNode.Tell(remoteConnect, Program.NeoSystem.LocalNode);

            var remoteNodes = LocalNode.Singleton.GetRemoteNodes();
            var success = remoteNodes.Any(rn => rn.Remote.Address.ToString().ToMatchedIp() == ip);

            return this.Ok(success);
        }
    }
}
