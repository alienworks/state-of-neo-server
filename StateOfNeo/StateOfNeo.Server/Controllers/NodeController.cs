using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using StateOfNeo.Server.Cache;
using StateOfNeo.Server.Hubs;
using StateOfNeo.Server.Infrastructure;
using StateOfNeo.ViewModels;
using StateOfNeo.Common.Extensions;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using StateOfNeo.Services;
using StateOfNeo.ViewModels.Chart;
using Serilog;
using StateOfNeo.Common.Constants;
using StateOfNeo.Common.RPC;
using StateOfNeo.Data;
using Neo.Wallets;
using Neo.Cryptography.ECC;
using Neo.SmartContract;
using System.Collections.Generic;
using StateOfNeo.Data.Models;
using Neo;
using Microsoft.EntityFrameworkCore;

namespace StateOfNeo.Server.Controllers
{
    [ResponseCache(Duration = CachingConstants.Hour)]
    public class NodeController : BaseApiController
    {
        private readonly NodeCache nodeCache;
        private readonly IHubContext<PeersHub> peersHub;
        private readonly INodeService nodeService;
        private readonly IStateService state;
        private readonly StateOfNeoContext db;

        public NodeController(
            StateOfNeoContext db,
            NodeCache nodeCache,
            IHubContext<PeersHub> peersHub,
            INodeService nodeService,
            IStateService state)
        {
            this.db = db;
            this.nodeCache = nodeCache;
            this.peersHub = peersHub;
            this.nodeService = nodeService;
            this.state = state;
        }

        [HttpGet("[action]/{id}")]
        [ResponseCache(Duration = CachingConstants.Second)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var node = this.nodeService.Get<NodeDetailsViewModel>(id);
                return Ok(node);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Get()
        {
            try
            {
                var nodes = this.nodeService.GetNodes<NodeDetailsViewModel>();
                var listNodes = nodes.ToList();
                return Ok(nodes);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Update()
        {
            try
            {
                //await this.nodeSynchronizer.Init();
                return Ok();
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("[action]")]
        [ResponseCache(Duration = CachingConstants.Second)]
        public async Task<IActionResult> Page(int page = 1, int pageSize = 10)
        {
            try
            {
                var nodes = await this.nodeService.GetPage<NodeViewModel>(page, pageSize);
                return Ok(nodes.ToListResult());
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Consensus()
        {
            try
            {
                Log.Information("Getting Consensus data");
                HttpResponseMessage response;

                using (var http = new HttpClient())
                {
                    var method = HttpMethod.Get;
                    var req = new HttpRequestMessage(method, $"https://neo.org/consensus/getvalidators");

                    response = await http.SendAsync(req);
                }

                if (response.IsSuccessStatusCode)
                {
                    var text = await response.Content.ReadAsStringAsync();
                    var list = JsonConvert.DeserializeObject<object[]>(text);
                    return Ok(list);
                }
                else
                {
                    Log.Error("Couldn't load consensus list neo.org bad request");
                    return BadRequest();
                }
            }
            catch (System.Exception ex)
            {
                Log.Error("Couldn't load consensus list {@ex}", ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("[action]/{nodeId}")]
        public IActionResult LatencyChart([FromBody]ChartFilterViewModel filter, int nodeId)
        {
            var result = this.nodeService.LatencyChart(filter, nodeId);
            return Ok(result);
        }

        [HttpPost("[action]/{nodeId}")]
        public IActionResult PeersChart([FromBody]ChartFilterViewModel filter, int nodeId)
        {
            var result = this.nodeService.PeersChart(filter, nodeId);
            return Ok(result);
        }

        [HttpGet("[action]/{nodeId}")]
        public async Task<IActionResult> WsStatus(int nodeId)
        {
            var status = await this.nodeService.GetWsStatusAsync(nodeId);
            return this.Ok(status);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetRPCNodes()
        {
            return this.Ok(this.nodeCache.PeersCollected);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> AddRPCNodes([FromBody]RPCPeer[] peers)
        {
            if (peers != null && peers.Length > 0)
            {
                var oldCollectedPeersCount = this.nodeCache.PeersCollected.Count;

                foreach (var peer in peers)
                {
                    this.nodeCache.AddPeer(peer);
                }

                if (oldCollectedPeersCount < this.nodeCache.PeersCollected.Count)
                {
                    await this.peersHub.Clients.All.SendAsync("total-found", this.nodeCache.PeersCollected.Count);
                }
            }

            return this.Ok();
        }

        [HttpPost("[action]")]
        [ResponseCache(Duration = CachingConstants.Hour)]
        public IActionResult ConsensusRewardsChart([FromBody]ChartFilterViewModel filter)
        {
            var result = this.state.GetConsensusRewardsChart(filter.UnitOfTime, filter.Period);
            return this.Ok(result);
        }

        [HttpGet("[action]")]
        public IActionResult NodeTypesChart()
        {
            var result = this.nodeService.NodeTypesChart();
            return this.Ok(result);
        }

        [HttpPost("[action]")]
        public IActionResult CalculateConsensusFees()
        {
            return this.Ok();
            //var previousBlockHash = Neo.Ledger.Blockchain.Singleton.GetBlockHash(3224873);
            //var previousBlock = Neo.Ledger.Blockchain.Singleton.GetBlock(previousBlockHash);

            //var block = Neo.Ledger.Blockchain.Singleton.GetBlock(UInt256.Parse("f008b7f7db21aa6464ed1dd852ab3c4a9c27ca38392624f0694fb1c58064b2d8"));

            //var test1 = previousBlock.NextConsensus.ToAddress();


            //var a = 5;

            var assetsToRemove = this.db.Transactions
                .Where(x => x.Timestamp > 1)
                .Select(x => x.Assets.Where(z => z.AssetType != StateOfNeo.Common.Enums.AssetType.NEP5))
                .ToList();

            var blocks = this.db.Blocks
                .OrderBy(x => x.Height)
                .Take(200_000)
                .ToList();

            //var i = 0;
            //while (i < blocks.Count)
            //{
            //    var block = blocks[i];
            //    if (block.Height == 0)
            //    {
            //        i++;
            //        continue;
            //    }

            //    var bcBlock = Neo.Ledger.Blockchain.Singleton.GetBlock(UInt256.Parse(block.Hash));
            //    block.NextConsensusNodeAddress = bcBlock.NextConsensus.ToAddress();

            //    i++;
            //    if (i % 200_000 == 0)
            //    {
            //        this.db.SaveChanges();

            //        blocks = this.db.Blocks
            //            .OrderBy(x => x.Height)
            //            .Where(x => x.Height > block.Height)
            //            .Take(200_000)
            //            .ToList();

            //        i = 0;
            //    }
            //}

            //this.db.SaveChanges();

            var validators = new List<ConsensusNode>();
            //var blockFees = this.db.Blocks
            //    .Select(x => new
            //    {
            //        x.Height,
            //        Tx = x.Transactions
            //            .Where(z => z.Type == Neo.Network.P2P.Payloads.TransactionType.MinerTransaction)
            //            .Select(z => new
            //            {
            //                Address = z.GlobalOutgoingAssets.Select(q => q.ToAddressPublicAddress).FirstOrDefault(),
            //                Amount = z.GlobalOutgoingAssets.Select(q => q.Amount).FirstOrDefault()
            //            })
            //            .FirstOrDefault()
            //    })
            //    .Where(x => x.Tx.Amount > 0)
            //    .ToList();

            var blockFees = this.db.Transactions
                .Include(x => x.GlobalOutgoingAssets)
                .Where(x => x.Type == Neo.Network.P2P.Payloads.TransactionType.MinerTransaction)
                .Where(x => x.GlobalOutgoingAssets.Any())
                .SelectMany(x => x.GlobalOutgoingAssets)
                .GroupBy(x => x.ToAddressPublicAddress)
                .ToList();

            foreach (var item in blockFees)
            {
                var validator = validators.FirstOrDefault(x => x.Address == item.Key);
                if (validator == null)
                {
                    validator = new ConsensusNode { Address = item.Key };
                    validators.Add(validator);
                    this.db.ConsensusNodes.Add(validator);
                }

                foreach (var tx in item)
                {
                    var dbTx = Neo.Ledger.Blockchain.Singleton.GetTransaction(UInt256.Parse(tx.OutGlobalTransactionHash));
                    foreach (var outgoing in dbTx.Outputs)
                    {

                    }
                }

                var fees = item.Sum(x => x.Amount);
                validator.CollectedFees = fees;
            }

            //foreach (var item in blockFees)
            //{
            //    var validator = validators.FirstOrDefault(x => x.Address == item.Tx.Address);
            //    if (validator == null)
            //    {
            //        validator = new ConsensusNode
            //        {
            //            Address = item.Tx.Address
            //        };

            //        validators.Add(validator);
            //        this.db.ConsensusNodes.Add(validator);
            //    }

            //    validator.CollectedFees += item.Tx.Amount;
            //}

            //this.db.SaveChanges();

            return this.Ok();
        }
    }
}
