using Neo;
using Neo.SmartContract;
using Neo.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Actors.Notifications
{
    public class AppExecutionResult
    {
        public TriggerType Trigger { get; set; }
        public UInt160 ScriptHash { get; set; }
        public VMState VMState { get; set; }
        public Fixed8 GasConsumed { get; set; }
        public StackItem[] Stack { get; set; }
        public NotifyEventArgs[] Notifications { get; set; }
    }
}
