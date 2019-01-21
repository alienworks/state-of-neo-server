using System.Collections.Generic;

namespace StateOfNeo.Services
{
    public interface ISmartContractService
    {
        IEnumerable<T> GetAll<T>();
    }
}
