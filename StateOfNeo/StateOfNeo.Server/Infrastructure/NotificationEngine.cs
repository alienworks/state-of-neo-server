using Microsoft.AspNetCore.SignalR;
using Neo.Core;
using StateOfNeo.Common;
using StateOfNeo.Server.Cache;
using StateOfNeo.Server.Hubs;
using StateOfNeo.ViewModels;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Linq;
using System;
using StateOfNeo.Data;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Neo;
using Neo.Implementations.Blockchains.LevelDB;
using AutoMapper.QueryableExtensions;
using StateOfNeo.Data.Models;

namespace StateOfNeo.Server.Infrastructure
{
    public class NotificationEngine
    {
        private bool IsInitialBlockConnection = false;
        private int NeoBlocksWithoutNodesUpdate = 0;
        private ulong TotalTransactionCount = 0;
        private DateTime LastBlockReceiveTime = default(DateTime);
        private readonly StateOfNeoContext _ctx;
        private readonly IHubContext<NodeHub> _nodeHub;
        private readonly IHubContext<BlockHub> blockHub;
        private readonly NodeCache _nodeCache;
        private readonly IHubContext<TransactionCountHub> _transCountHub;
        private readonly IHubContext<FailedP2PHub> _failP2PHub;
        private readonly IHubContext<TransactionAverageCountHub> _transAvgCountHub;
        private readonly NodeSynchronizer _nodeSynchronizer;
        private readonly RPCNodeCaller _rPCNodeCaller;
        private readonly NetSettings _netSettings;
        private readonly PeersEngine _peersEngine;

        public NotificationEngine(IHubContext<NodeHub> nodeHub,
            IHubContext<BlockHub> blockHub,
            IHubContext<TransactionCountHub> transCountHub,
            IHubContext<TransactionAverageCountHub> transAvgCountHub,
            IHubContext<FailedP2PHub> failP2PHub,
            NodeCache nodeCache,
            PeersEngine peersEngine,
            NodeSynchronizer nodeSynchronizer,
            RPCNodeCaller rPCNodeCaller,
            StateOfNeoContext ctx,
            IOptions<NetSettings> netSettings)
        {
            _ctx = ctx;
            _nodeHub = nodeHub;
            _nodeCache = nodeCache;
            _transCountHub = transCountHub;
            _failP2PHub = failP2PHub;
            _transAvgCountHub = transAvgCountHub;
            _nodeSynchronizer = nodeSynchronizer;
            _rPCNodeCaller = rPCNodeCaller;
            _netSettings = netSettings.Value;
            _peersEngine = peersEngine;
            this.blockHub = blockHub;
        }

        public void Init()
        {
            Blockchain.PersistCompleted += Blockchain_PersistCompleted;
        }

        private void Blockchain_PersistCompleted(object sender, Neo.Core.Block e)
        {

        }
    }
}
