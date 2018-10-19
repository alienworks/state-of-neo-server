using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Neo.Ledger.Blockchain;

namespace StateOfNeo.Server.Actors
{
    public class NotificationsBroadcaster : UntypedActor
    {
        public NotificationsBroadcaster(IActorRef blockchain)
        {
            blockchain.Tell(new Register());
        }

        protected override void OnReceive(object message)
        {
            if (message is ApplicationExecuted m)
            {
                
            }
        }

        public static Props Props(IActorRef blockchain) =>
            Akka.Actor.Props.Create(() => new NotificationsBroadcaster(blockchain));
    }
}
