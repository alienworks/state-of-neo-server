using Microsoft.AspNetCore.Mvc;
using StateOfNeo.Common;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Controllers
{
    public class P2PStatusController : BaseApiController
    {
        [HttpPost("[action]/{ip}")]
        public async Task<IActionResult> CheckIp(string ip)
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
