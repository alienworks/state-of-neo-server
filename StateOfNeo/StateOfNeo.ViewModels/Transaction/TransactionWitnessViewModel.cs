using System;
using System.Collections.Generic;
using System.Text;

namespace StateOfNeo.ViewModels.Transaction
{
    public class TransactionWitnessViewModel
    {
        public string InvocationScriptAsHexString { get; set; }

        public string VerificationScriptAsHexString { get; set; }

        public string Address { get; set; }
    }
}
