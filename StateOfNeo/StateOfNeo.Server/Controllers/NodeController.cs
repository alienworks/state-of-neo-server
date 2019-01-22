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

namespace StateOfNeo.Server.Controllers
{
    [ResponseCache(Duration = CachingConstants.Hour)]
    public class NodeController : BaseApiController
    {
        private readonly NodeCache nodeCache;
        private readonly INodeService nodeService;
        private readonly StateOfNeoContext db;

        public NodeController(
            StateOfNeoContext db,
            NodeCache nodeCache,
            INodeService nodeService)
        {
            this.db = db;
            this.nodeCache = nodeCache;
            this.nodeService = nodeService;
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
        public async Task<IActionResult> AddRPCNodes(RPCPeer[] peers)
        {
            if (peers != null)
            {
                foreach (var peer in peers)
                {
                    this.nodeCache.AddPeer(peer);
                }
            }

            return this.Ok();
        }

        [HttpPost("[action]")]
        public IActionResult CalculateConsensusFees()
        {
            //var publicKeyStrings = new[] {
            //    "025bdf3f181f53e9696227843950deb72dcd374ded17c057159513c3d0abe20b64",
            //    "02df48f60e8f3e01c48ff40b9b7f1310d7a8b2a193188befe1c2e3df740e895093",
            //    "024c7b7fb6c310fccf1ba33b082519d82964ea93868d676662d4a59ad548df0e7d",
            //    "02ca0e27697b9c248f6f16e085fd0061e26f44da85b58ee835c110caa5ec3ba554",
            //    "035e819642a8915a2572f972ddbdbe3042ae6437349295edce9bdc3b8884bbf9a3",
            //    "03b209fd4f53a7170ea4444e0cb0a6bb6a53c2bd016926989cf85f9b0fba17a70c",
            //    "03b8d9d5771d8f513aa0869b9cc8d50986403b78c6da36890638c3d46a5adce04a"
            //};

            //foreach (var item in publicKeyStrings)
            //{
            //    var publicKey = ECPoint.Parse(item, ECCurve.Secp256r1);
            //    var publicKeyHash = Contract.CreateSignatureRedeemScript(publicKey).ToScriptHash();
            //    var hashStr = publicKeyHash.ToString();
            //    var address = publicKeyHash.ToAddress();

            //    var validator = new Data.Models.ConsensusNode
            //    {
            //        PublicKeyHash = publicKeyHash.ToString(),
            //        Address = address,
            //        PublicKey = item
            //    };

            //    this.db.ConsensusNodes.Add(validator);
            //}

            //this.db.SaveChanges();

            var blocks = this.db.Blocks
                .OrderBy(x => x.Height)
                .Select(x => new { Block = x, Fees = x.Transactions.Sum(z => z.SystemFee) })
                .ToList();

            var validators = new List<ConsensusNode>();
            for (int i = 1; i < blocks.Count; i++)
            {
                var previousHash = Neo.Ledger.Blockchain.Singleton.GetBlockHash((uint)i - 1);
                var previousBlock = Neo.Ledger.Blockchain.Singleton.GetBlock(previousHash);
                blocks[i].Block.Validator = previousBlock.NextConsensus.ToAddress();

                var validator = validators.FirstOrDefault(x => x.Address == blocks[i].Block.Validator);
                if (validator == null)
                {
                    validator = new ConsensusNode
                    {
                        Address = blocks[i].Block.Validator
                    };

                    validators.Add(validator);
                    this.db.ConsensusNodes.Add(validator);
                    this.db.SaveChanges();
                }

                if (i % 50_000 == 0)
                {
                    this.db.SaveChanges();
                }
            }

            this.db.SaveChanges();

            return this.Ok();
        }
    }
}
