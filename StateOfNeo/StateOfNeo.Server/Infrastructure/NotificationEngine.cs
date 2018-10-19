using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Neo.Ledger;
using Neo.Wallets;
using StateOfNeo.Common;
using StateOfNeo.Data;
using StateOfNeo.Data.Models;
using StateOfNeo.Data.Models.Enums;
using StateOfNeo.Server.Actors;
using StateOfNeo.Server.Cache;
using StateOfNeo.Server.Hubs;
using System;
using System.Linq;

namespace StateOfNeo.Server.Infrastructure
{
    public class NotificationEngine
    {
        private readonly IHubContext<NodeHub> nodeHub;
        private readonly IHubContext<BlockHub> blockHub;
        private readonly NodeCache nodeCache;
        private readonly IHubContext<TransactionCountHub> transCountHub;
        private readonly IHubContext<FailedP2PHub> failP2PHub;
        private readonly IHubContext<TransactionAverageCountHub> transAvgCountHub;
        private readonly NodeSynchronizer nodeSynchronizer;
        private readonly RPCNodeCaller rPCNodeCaller;
        private readonly NetSettings netSettings;
        private readonly PeersEngine peersEngine;

        public NotificationEngine(
            IHubContext<NodeHub> nodeHub,
            IHubContext<BlockHub> blockHub,
            IHubContext<TransactionCountHub> transCountHub,
            IHubContext<TransactionAverageCountHub> transAvgCountHub,
            IHubContext<FailedP2PHub> failP2PHub,
            NodeCache nodeCache,
            PeersEngine peersEngine,
            NodeSynchronizer nodeSynchronizer,
            RPCNodeCaller rPCNodeCaller,
            IOptions<NetSettings> netSettings)
        {
            this.nodeHub = nodeHub;
            this.nodeCache = nodeCache;
            this.transCountHub = transCountHub;
            this.failP2PHub = failP2PHub;
            this.transAvgCountHub = transAvgCountHub;
            this.nodeSynchronizer = nodeSynchronizer;
            this.rPCNodeCaller = rPCNodeCaller;
            this.netSettings = netSettings.Value;
            this.peersEngine = peersEngine;
            this.blockHub = blockHub;
        }

        public void Init()
        {


            //NotificationsBroadcaster.ApplicationExecuted += NotificationsBroadcaster_ApplicationExecuted;
        }

        private void NotificationsBroadcaster_ApplicationExecuted(object sender, Neo.Ledger.Blockchain.ApplicationExecuted e)
        {

        }
    }
}
