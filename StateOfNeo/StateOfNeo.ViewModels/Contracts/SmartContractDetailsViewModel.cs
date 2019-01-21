using Neo.SmartContract;
using System;
using System.Collections.Generic;
using System.Text;

namespace StateOfNeo.ViewModels.Contracts
{
    public class SmartContractDetailsViewModel
    {
        public string Hash { get; set; }

        public long Timestamp { get; set; }

        public string Name { get; set; }

        public string Author { get; set; }

        public string Version { get; set; }

        public string Description { get; set; }

        public string Email { get; set; }

        public bool HasStorage { get; set; }

        public bool Payable { get; set; }

        public bool HasDynamicInvoke { get; set; }

        public string InputParameters { get; set; }

        public ContractParameterType ReturnType { get; set; }
    }
}
