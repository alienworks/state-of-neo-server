using Akka.Actor;
using Neo.Ledger;
using System;

namespace StateOfNeo.Server.Infrastructure
{
    public class NotificationBroadcaster : UntypedActor
    {
        public static event EventHandler<Blockchain.ApplicationExecuted> ApplicationExecuted;

        public NotificationBroadcaster(IActorRef blockchain)
        {
            blockchain.Tell(new Blockchain.Register());
        }

        protected override void OnReceive(object message)
        {
            if (message is Blockchain.ApplicationExecuted e)
            {
                var test = e.ExecutionResults;
            }
        }

        public static Props Props(IActorRef blockchain)
        {
            return Akka.Actor.Props.Create(() => new NotificationBroadcaster(blockchain));
        }
    }
}
