using StateOfNeo.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StateOfNeo.Data.Models
{
    public class Node : BaseEntity
    {
        public Node()
        {
            this.NodeAddresses = new HashSet<NodeAddress>();
            this.NodeStatusUpdates = new HashSet<NodeStatus>();
            this.Audits = new HashSet<NodeAudit>();
        }

        [Key]
        public int Id { get; set; }

        public bool IsHttps { get; set; }

        public string Protocol { get; set; }
        public string Url { get; set; }
        public string SuccessUrl { get; set; }
        public string Net { get; set; }

        public int? Height { get; set; }
        public int? Peers { get; set; }
        public int? MemoryPool { get; set; }
        public string Version { get; set; }
        public NodeAddressType Type { get; set; }
        public string Locale { get; set; }
        public string Location { get; set; }
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
        public string FlagUrl { get; set; }

        public long? FirstRuntime { get; set; }
        public long? LatestRuntime { get; set; }
        public long SecondsOnline { get; set; }
        public long? LastAudit { get; set; }

        public virtual ICollection<NodeAddress> NodeAddresses { get; set; }

        public virtual ICollection<NodeStatus> NodeStatusUpdates { get; set; }

        public virtual ICollection<NodeAudit> Audits { get; private set; }
    }
}
