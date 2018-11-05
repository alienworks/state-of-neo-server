using Akka.Actor;
using Neo.Ledger;
using Neo.Network.P2P.Payloads;
using Neo.SmartContract;
using StateOfNeo.Common.Extensions;
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
                var transaction = m.Transaction as InvocationTransaction;
                foreach (var result in m.ExecutionResults)
                {
                    foreach (var item in result.Notifications)
                    {
                        var type = item.GetNotificationType();
                        if (type == "transfer")
                        {
                            var contractHash = item.ScriptHash;


                        }
                    }
                }
            }
        }

        public static Props Props(IActorRef blockchain) =>
            Akka.Actor.Props.Create(() => new NotificationsBroadcaster(blockchain));
    }
}
