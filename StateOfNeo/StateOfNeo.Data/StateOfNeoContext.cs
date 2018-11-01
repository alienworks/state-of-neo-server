using Microsoft.EntityFrameworkCore;
using StateOfNeo.Data.Models;
using StateOfNeo.Data.Models.Transactions;
using System;

namespace StateOfNeo.Data
{
    public class StateOfNeoContext : DbContext
    {
        public StateOfNeoContext(DbContextOptions<StateOfNeoContext> options)
            : base(options)
        {

        }

        public DbSet<Address> Addresses { get; set; }
        public DbSet<AddressAssetBalance> AddressBalances { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<Block> Blocks { get; set; }
        public DbSet<Node> Nodes { get; set; }
        public DbSet<NodeAddress> NodeAddresses { get; set; }
        public DbSet<NodeStatusUpdate> NodeStatusUpdates { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransactionAttribute> TransactionAttributes { get; set; }
        public DbSet<TransactionWitness> TransactionWitnesses { get; set; }
        public DbSet<TransactedAsset> TransactedAssets { get; set; }
        public DbSet<EnrollmentTransaction> EnrollmentTransactions { get; set; }
        public DbSet<InvocationTransaction> InvocationTransactions { get; set; }
        public DbSet<PublishTransaction> PublishTransactions { get; set; }
        public DbSet<RegisterTransaction> RegisterTransactions { get; set; }
        public DbSet<StateTransaction> StateTransactions { get; set; }
        public DbSet<StateDescriptor> StateDescriptors { get; set; }
    }
}
