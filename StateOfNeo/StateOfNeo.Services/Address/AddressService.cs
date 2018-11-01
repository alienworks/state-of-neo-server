using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using StateOfNeo.Common.Constants;
using StateOfNeo.Common.Enums;
using StateOfNeo.Common.Extensions;
using StateOfNeo.Data;
using StateOfNeo.ViewModels.Address;

namespace StateOfNeo.Services.Address
{
    public class AddressService : IAddressService
    {
        private readonly StateOfNeoContext db;

        public AddressService(StateOfNeoContext db)
        {
            this.db = db;
        }

        public int CreatedAddressesCount() => this.db.Addresses.Count();

        public int ActiveAddressesInThePastThreeMonths() =>
            this.db.Addresses
                .Include(x => x.OutgoingTransactions).ThenInclude(tr => tr.Transaction).ThenInclude(x => x.Block)
                .Where(x => x.OutgoingTransactions.Any(tr => tr.Transaction.Block.Timestamp.ToCurrentDate() > DateTime.UtcNow.AddMonths(-3)))
                .Count();

        public int CreatedAddressesPer(TimePeriod timePeriod)
        {
            var result = 0;
            if (timePeriod == TimePeriod.Hour)
            {
                result = this.db.Addresses.Count() / this.db.Addresses
                       .GroupBy(x => new { x.FirstTransactionOn.Year, x.FirstTransactionOn.Month, x.FirstTransactionOn.Day, x.FirstTransactionOn.Hour })
                       .Count();
            }
            else if (timePeriod == TimePeriod.Day)
            {
                result = this.db.Addresses.Count() / this.db.Addresses
                       .GroupBy(x => new { x.FirstTransactionOn.Year, x.FirstTransactionOn.Month, x.FirstTransactionOn.Day })
                       .Count();
            }
            else if (timePeriod == TimePeriod.Month)
            {
                result = this.db.Addresses.Count() / this.db.Addresses
                    .GroupBy(x => new { x.FirstTransactionOn.Year, x.FirstTransactionOn.Month })
                    .Count();
            }

            return result;
        }

        public IEnumerable<PublicAddressListViewModel> TopOneHundred()
        {
            var result = this.db.Addresses
                .Include(x => x.IncomingTransactions).ThenInclude(x => x.Asset)
                .Include(x => x.OutgoingTransactions).ThenInclude(x => x.Asset)
                .Include(x => x.OutgoingTransactions).ThenInclude(x => x.Transaction)
                .Select(x => new
                {
                    Address = x.PublicAddress,
                    NeoIncome = x.IncomingTransactions.Any(tr => tr.Asset.Hash == AssetConstants.NeoAssetId) 
                        ? x.IncomingTransactions.Where(tr => tr.Asset.Hash == AssetConstants.NeoAssetId).Sum(tr => tr.Amount)
                        : 0,
                    GasIncome = (x.IncomingTransactions.Any(tr => tr.Asset.Hash == AssetConstants.GasAssetId)
                        ? x.IncomingTransactions.Where(tr => tr.Asset.Hash == AssetConstants.GasAssetId).Sum(tr => tr.Amount)
                        : 0),
                    NeoOutcome = x.OutgoingTransactions.Any(tr => tr.Asset.Hash == AssetConstants.NeoAssetId)
                        ? x.OutgoingTransactions.Where(tr => tr.Asset.Hash == AssetConstants.NeoAssetId).Sum(tr => tr.Amount)
                        : 0,
                    GasOutcome = x.OutgoingTransactions.Any(tr => tr.Asset.Hash == AssetConstants.GasAssetId)
                        ? x.OutgoingTransactions.Where(tr => tr.Asset.Hash == AssetConstants.GasAssetId).Sum(tr => tr.Amount)
                        : 0,
                    GasFees = x.OutgoingTransactions.Any()
                        ? x.OutgoingTransactions.Select(tr => tr.Transaction.SystemFee).Sum()
                        : 0
                })
                .Select(x => new PublicAddressListViewModel
                {
                    Address = x.Address,
                    NeoBalance = x.NeoIncome - x.NeoOutcome,
                    GasBalance = x.GasIncome - x.GasOutcome - x.GasFees
                })
                .OrderByDescending(x => x.NeoBalance)
                .Take(100)
                .ToList();

            return result;
        }
    }
}
