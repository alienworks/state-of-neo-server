using System.Threading.Tasks;
using X.PagedList;

namespace StateOfNeo.Services
{
    public interface INodeService
    {
        Task<IPagedList<T>> GetPage<T>(int page = 1, int pageSize = 10);
    }
}
