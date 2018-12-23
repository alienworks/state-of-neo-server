using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StateOfNeo.Common;
using StateOfNeo.Data;
using StateOfNeo.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Infrastructure
{
    public class LocationCaller
    {
        private Dictionary<string, Tuple<string, string, double, double>> IPs = new Dictionary<string, Tuple<string, string, double, double>>();

        private StateOfNeoContext ctx;

        public LocationCaller(StateOfNeoContext ctx)
        {
            this.ctx = ctx;
        }

        public async Task UpdateAllNodeLocations()
        {
            var addresses = this.ctx.NodeAddresses
                .Include(adr => adr.Node)
                .Where(adr => !adr.Node.Latitude.HasValue || !adr.Node.Longitude.HasValue)
                .ToList();

            foreach (var address in addresses)
            {
                var node = this.ctx.Nodes
                    .FirstOrDefault(n => n.Id == address.NodeId);
                await this.UpdateNode(node, address.Ip);
            }
        }

        public async Task<bool> UpdateNodeLocation(int nodeId)
        {
            var node = this.ctx.Nodes
                .Include(n => n.NodeAddresses)
                .FirstOrDefault(n => n.Id == nodeId);

            if (node != null && node.NodeAddresses.Count() > 0)
            {
                foreach (var address in node.NodeAddresses)
                {
                    var result = await this.UpdateNode(node, address.Ip);
                    if (result)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public async Task<bool> UpdateNode(Node node, string ip)
        {
            try
            {
                if (node.Latitude == null || node.Longitude == null)
                {

                    var response = await this.CheckIpCall(ip);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseText = await response.Content.ReadAsStringAsync();
                        var responseOject = JsonConvert.DeserializeObject<IpCheckModel>(responseText);

                        node.FlagUrl = responseOject.Location.Flag;
                        node.Location = responseOject.CountryName;
                        node.Latitude = responseOject.Latitude;
                        node.Longitude = responseOject.Longitude;
                        node.Locale = responseOject.Location.Languages.FirstOrDefault().Code;

                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                return false;
            }

            return false;
        }

        private async Task<HttpResponseMessage> CheckIpCall(string ip)
        {
            HttpResponseMessage response;
            try
            {
                using (var http = new HttpClient())
                {
                    var req = new HttpRequestMessage(HttpMethod.Get, $"http://api.ipstack.com/{ip}?access_key=86e45b940f615f26bba14dde0a002bc3");
                    response = await http.SendAsync(req);
                }
            }
            catch (Exception e)
            {
                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            return response;
        }
    }
}
