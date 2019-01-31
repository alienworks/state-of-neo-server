using StateOfNeo.ViewModels.Contracts;
using System.Collections.Generic;
using X.PagedList;

namespace StateOfNeo.Services
{
    public interface ISmartContractService
    {
        SmartContractDetailsViewModel Find(string hash);

        IEnumerable<T> GetAll<T>();
        IPagedList<T> GetTransactions<T>(string hash, int page, int pageSize);
    }
}
