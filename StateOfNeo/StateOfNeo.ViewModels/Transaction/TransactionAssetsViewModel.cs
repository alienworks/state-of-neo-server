using System;
using System.Collections.Generic;
using System.Linq;

namespace StateOfNeo.ViewModels.Transaction
{
    public class TransactionAssetsViewModel
    {
        public IEnumerable<TransactedAssetViewModel> GlobalIncomingAssets { get; set; }

        public IEnumerable<TransactedAssetViewModel> GlobalOutgoingAssets { get; set; }

        public IEnumerable<TransactedAssetViewModel> Assets { get; set; }

        public IEnumerable<TransactedAssetViewModel> SentAssets =>
            this.GlobalIncomingAssets
                .GroupBy(x => new { x.FromAddress, x.AssetType })
                .Select(x => new TransactedAssetViewModel
                {
                    FromAddress = x.Key.FromAddress,
                    Amount = x.Sum(z => z.Amount),
                    AssetType = x.Key.AssetType,
                    Name = x.First().Name,
                    AssetHash = x.First().AssetHash,
                    AssetSymbol = x.First().AssetSymbol
                })
                .ToList();

        public IEnumerable<TransactedAssetViewModel> ReceivedAssets =>
            this.GlobalOutgoingAssets
                .GroupBy(x => new { x.ToAddress, x.AssetType })
                .Select(x => new TransactedAssetViewModel
                {
                    ToAddress = x.Key.ToAddress,
                    Amount = x.Sum(z => z.Amount),
                    AssetType = x.Key.AssetType,
                    Name = x.First().Name,
                    AssetHash = x.First().AssetHash,
                    AssetSymbol = x.First().AssetSymbol
                })
                .ToList();

        public IEnumerable<TransactedAssetViewModel> AssetsWithNewOwner =>
            this.ReceivedAssets
                .Where(x => !this.SentAssets.Any(a => a.AssetHash == x.AssetHash && a.FromAddress == x.ToAddress))
                .ToList();
    }
}
