using System;
using System.Collections.Generic;

namespace StateOfNeo.Services
{
    public interface ISearchService
    {
        KeyValuePair<String, String> Find(string input);
    }
}
