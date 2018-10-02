using Microsoft.AspNetCore.Mvc;
using StateOfNeo.Common;
using StateOfNeo.Server.Infrastructure;
using StateOfNeo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace StateOfNeo.Server.Controllers
{
    public class PeersController : BaseApiController
    {
        private readonly PeersEngine _peersEngine;

        public PeersController(PeersEngine peersEngine)
        {
            _peersEngine = peersEngine;
        }

        [HttpPost]
        public IActionResult Post(ICollection<IPEndPointViewModel> peers)
        {
            try
            {
                var newPeers = new List<IPEndPoint>();
                foreach (var peer in peers)
                {
                    newPeers.Add(new IPEndPoint(IPAddress.Parse(peer.Address.ToMatchedIp()), peer.Port));
                }
                _peersEngine.AddNewPeers(newPeers);
                _peersEngine.UpdateClients();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
