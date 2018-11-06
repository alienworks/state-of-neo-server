using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Actors.Notifications
{
    public class TransferNotification
    {
        public byte[] From { get; set; }

        public byte[] To { get; set; }

        public BigInteger Amount { get; set; }
    }
}
