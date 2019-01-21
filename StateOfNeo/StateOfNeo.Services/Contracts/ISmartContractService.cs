using StateOfNeo.ViewModels.Contracts;
using System.Collections.Generic;

namespace StateOfNeo.Services
{
    public interface ISmartContractService
    {
        SmartContractDetailsViewModel Find(string hash);

        IEnumerable<T> GetAll<T>();
    }
}
