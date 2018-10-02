using System;
using System.Collections.Generic;
using System.Text;

namespace StateOfNeo.Data.Models
{
    public class NodeStatusUpdate : BaseEntity
    {
        public int Id { get; set; }

        public bool IsRpcOnline { get; set; }

        public bool IsP2pTcpOnline { get; set; }

        public bool IsP2pWsOnline { get; set; }

        public int BlockId { get; set; }

        public virtual Block Block { get; set; }

        public int NodeId { get; set; }

        public virtual Node Node { get; set; }
    }
}
