using StateOfNeo.Common;
using StateOfNeo.Common.Enums;
using StateOfNeo.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateOfNeo.Services
{
    public class SearchService : ISearchService
    {
        private readonly StateOfNeoContext db;
        private readonly IMainStatsState mainStats;

        public SearchService(StateOfNeoContext db, IMainStatsState mainStats)
        {
            this.db = db;
            this.mainStats = mainStats;
        }

        public KeyValuePair<string, string> Find(string input)
        {
            var result = default(KeyValuePair<string, string>);

            if (int.TryParse(input, out int blockHeight))
            {
                if (blockHeight > 0 &&
                    this.mainStats.GetTotalBlocksCount() >= blockHeight &&
                    this.db.Blocks.Any(x => x.Height == blockHeight))
                {
                    result = input.ToKeyValueWithKey("block");
                }
            }
            else
            {
                if (!input.StartsWith("0x") && input.Length != 34)
                {
                    input = "0x" + input;
                }

                if (input.Length == 66)
                {
                    // check if string is hexstring

                    // can be global asset, block or tx hash we start from the shortest db set to the biggest
                    // for performance purposes

                    if (this.db.Assets.Any(x => x.Hash == input))
                    {
                        result = input.ToKeyValueWithKey("asset");
                    }
                    else if (this.db.Blocks.Any(x => x.Hash == input))
                    {
                        result = input.ToKeyValueWithKey("block");
                    }
                    else if (this.db.Transactions.Any(x => x.Hash == input))
                    {
                        result = input.ToKeyValueWithKey("transaction");
                    }
                }
                else if (input.Length == 42)
                {
                    if (this.db.Assets.Any(x => x.Hash == input))
                    {
                        result = input.ToKeyValueWithKey("asset");
                    }
                }
                else if (input.Length == 34)
                {
                    if (this.db.Addresses.Any(x => x.PublicAddress == input))
                    {
                        result = input.ToKeyValueWithKey("address");
                    }
                }
            }

            return result;
        }
    }
}
