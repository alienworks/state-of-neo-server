using Microsoft.AspNetCore.Mvc;
using StateOfNeo.Common;
using StateOfNeo.Node.Infrastructure;
using StateOfNeo.ViewModels;
using System;
using System.Collections.Generic;
using System.Net;

namespace StateOfNeo.Node.Controllers
{
    public class PeersController : BaseApiController
    {
        private readonly PeersEngine peersEngine;

        public PeersController(PeersEngine peersEngine)
        {
            this.peersEngine = peersEngine;
        }

        [HttpPost]
        public IActionResult Post(ICollection<IPEndPointViewModel> peers)
        {
            try
            {
                var newPeers = new List<IPEndPoint>();
                foreach (var peer in peers)
                {
                    var ip = IPAddress.Parse(peer.Address.ToMatchedIp());
                    newPeers.Add(new IPEndPoint(ip, peer.Port));
                }

                this.peersEngine.AddNewPeers(newPeers);
                this.peersEngine.UpdateClients();

                return this.Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
