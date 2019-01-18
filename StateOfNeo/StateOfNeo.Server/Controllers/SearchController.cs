using Microsoft.AspNetCore.Mvc;
using StateOfNeo.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Controllers
{
    public class SearchController : BaseApiController
    {
        private readonly ISearchService search;

        public SearchController(ISearchService search)
        {
            this.search = search;
        }

        [HttpGet("[action]/{input}")]
        public async Task<IActionResult> Find(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return BadRequest();
            }

            var result = this.search.Find(input);

            return Ok(new { route = result.Key, value = result.Value });
        }
    }
}
