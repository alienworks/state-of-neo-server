using Microsoft.AspNetCore.Mvc;
using StateOfNeo.Common;
using StateOfNeo.Server.Infrastructure;
using StateOfNeo.Services;
using StateOfNeo.ViewModels;
using System;
using System.Collections.Generic;
using System.Net;

namespace StateOfNeo.Server.Controllers
{
    public class PeersController : BaseApiController
    {
        private readonly PeersEngine peersEngine;
        private readonly IPeerService peers;

        public PeersController(PeersEngine peersEngine, IPeerService peers)
        {
            this.peersEngine = peersEngine;
            this.peers = peers;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return this.Ok(this.peers.GetAll<PeerViewModel>());
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
                return this.BadRequest(ex.Message);
            }
        }
    }
}
