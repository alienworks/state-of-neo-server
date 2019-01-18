using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StateOfNeo.Common;
using StateOfNeo.Common.Constants;
using StateOfNeo.Common.Http;
using StateOfNeo.Common.RPC;
using StateOfNeo.Data;
using StateOfNeo.Data.Models;
using StateOfNeo.Server.Cache;
using StateOfNeo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Infrastructure
{
    public class NodeSynchronizer
    {
        private readonly StateOfNeoContext db;
        private readonly RPCNodeCaller rPCNodeCaller;
        private readonly IOptions<NetSettings> netsettings;

        public NodeSynchronizer(
            RPCNodeCaller rPCNodeCaller,
            IOptions<NetSettings> netsettings,
            IOptions<DbSettings> dbSettings)
        {
            this.db = StateOfNeoContext.Create(dbSettings.Value.DefaultConnection);
            this.rPCNodeCaller = rPCNodeCaller;
            this.netsettings = netsettings;
        }

        public async Task Init()
        {
            await this.UpdateNodesInformation();
        }

        private async Task UpdateNodesInformation()
        {
            var dbNodes = this.db.Nodes
                    .Include(n => n.NodeAddresses)
                    .Where(n => n.Net.ToLower() == netsettings.Value.Net.ToLower())
                    .Where(n => n.SuccessUrl == null)
                    //.Where(n => n.Type != NodeAddressType.REST)
                    .ToList();

            foreach (var dbNode in dbNodes)
            {
                if (dbNode.Type == NodeAddressType.REST)
                {
                    if (dbNode.Service == NodeCallsConstants.NeoScan)
                    {
                        var heightResponse = await HttpRequester.MakeRestCall<HeightResponseObject>($@"{dbNode.Url}get_height", HttpMethod.Get);

                        if (heightResponse != null)
                        {
                            dbNode.SuccessUrl = dbNode.Url;
                            dbNode.Height = heightResponse.Height;

                            var result = await LocationCaller.UpdateNode(dbNode, dbNode.NodeAddresses.First().Ip);

                            if (result)
                            {
                                this.db.Nodes.Update(dbNode);
                                this.db.SaveChanges();
                            }
                        }
                    }
                    else if (dbNode.Service == NodeCallsConstants.NeoNotification)
                    {
                        var versionResponse = await HttpRequester.MakeRestCall<NeoNotificationVersionResponse>($@"{dbNode.Url}version", HttpMethod.Get);

                        if (versionResponse != null)
                        {
                            dbNode.SuccessUrl = dbNode.Url;
                            dbNode.Version = versionResponse.Version;
                            dbNode.Height = versionResponse.Height;

                            var result = await LocationCaller.UpdateNode(dbNode, dbNode.NodeAddresses.First().Ip);

                            if (result)
                            {
                                this.db.Nodes.Update(dbNode);
                                this.db.SaveChanges();
                            }
                        }
                    }
                }
                else if (dbNode.Type == NodeAddressType.RPC)
                {
                    var oldSuccessUrl = dbNode.SuccessUrl;
                    var newHeight = await this.rPCNodeCaller.GetNodeHeight(dbNode);

                    if (newHeight != null)
                    {
                        var newVersion = await this.rPCNodeCaller.GetNodeVersion(dbNode);

                        if (newVersion != null)
                        {
                            dbNode.Version = newVersion;
                            dbNode.Height = newHeight;

                            var result = await LocationCaller.UpdateNode(dbNode, dbNode.NodeAddresses.First().Ip);

                            if (result)
                            {
                                if (string.IsNullOrEmpty(dbNode.Net))
                                {
                                    dbNode.Net = netsettings.Value.Net;
                                }

                                this.db.Nodes.Update(dbNode);
                                this.db.SaveChanges();
                            }

                            this.db.Nodes.Update(dbNode);
                            this.db.SaveChanges();
                        }
                    }
                }
            }
        }
    }
}
