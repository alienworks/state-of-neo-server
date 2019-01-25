using System.Collections.Generic;

namespace StateOfNeo.Services
{
    public interface IPeerService
    {
        IEnumerable<T> GetAll<T>();
    }
}
