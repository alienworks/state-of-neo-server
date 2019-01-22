using Microsoft.EntityFrameworkCore;
using StateOfNeo.Common.Extensions;
using StateOfNeo.Data.Models;
using StateOfNeo.Data.Models.Transactions;
using System;
using System.Linq;

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
        public DbSet<AddressInTransaction> AddressesInTransactions { get; set; }
        public DbSet<AddressInAssetTransaction> AddressesInAssetTransactions { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<AssetInTransaction> AssetsInTransactions { get; set; }
        public DbSet<Block> Blocks { get; set; }
        public DbSet<Node> Nodes { get; set; }
        public DbSet<Peer> Peers { get; set; }
        public DbSet<NodeAddress> NodeAddresses { get; set; }
        public DbSet<NodeStatus> NodeStatusUpdates { get; set; }
        public DbSet<NodeAudit> NodeAudits { get; set; }
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
        public DbSet<ChartEntry> ChartEntries { get; set; }
        public DbSet<TotalStats> TotalStats { get; set; }
        public DbSet<SmartContract> SmartContracts { get; set; }
        public DbSet<ConsensusNode> ConsensusNodes { get; set; }

        public static StateOfNeoContext Create(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<StateOfNeoContext>();
            optionsBuilder.UseSqlServer(connectionString, 
                opts => opts.CommandTimeout((int)TimeSpan.FromMinutes(10000).TotalSeconds));
            return new StateOfNeoContext(optionsBuilder.Options);
        }

        public override int SaveChanges()
        {
            this.ApplyAuditInfoRules();
            this.ApplyStampedEntityRules();

            return base.SaveChanges();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Address>().HasIndex(x => x.LastTransactionOn);

            modelBuilder.Entity<Block>().HasIndex(x => x.Timestamp);
            modelBuilder.Entity<Block>().HasIndex(x => x.Height);

            modelBuilder.Entity<Transaction>().HasIndex(x => x.Timestamp);
            modelBuilder.Entity<Transaction>().HasIndex(x => new { x.Timestamp, x.Hash });

            modelBuilder.Entity<AssetInTransaction>().HasIndex(x => new { x.Timestamp, x.AssetHash });

            modelBuilder.Entity<NodeAudit>().HasIndex(x => x.Timestamp);
            modelBuilder.Entity<NodeAudit>()
                .HasOne(x => x.Node)
                .WithMany(x => x.Audits)
                .HasForeignKey(x => x.NodeId);

            modelBuilder.Entity<Node>()
                .HasMany(x => x.Audits)
                .WithOne(x => x.Node)
                .HasForeignKey(x => x.NodeId);

            var decimalProps = modelBuilder.Model
                .GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal));

            foreach (var property in decimalProps)
            {
                property.Relational().ColumnType = "decimal(36, 8)";
            }
        }

        private void ApplyAuditInfoRules()
        {
            var addedBaseEntities = this.ChangeTracker
                .Entries()
                .Where(e => e.Entity is BaseEntity && e.State == EntityState.Added);

            foreach (var entry in addedBaseEntities)
            {
                var entity = (BaseEntity)entry.Entity;
                if (entry.State == EntityState.Added && entity.CreatedOn == default(DateTime))
                {
                    entity.CreatedOn = DateTime.UtcNow;
                }
            }
        }

        private void ApplyStampedEntityRules()
        {
            var addedStampedEntities = this.ChangeTracker
                .Entries()
                .Where(e => e.Entity is StampedEntity && e.State == EntityState.Added);

            foreach (var entry in addedStampedEntities)
            {
                var entity = (StampedEntity)entry.Entity;
                if (entry.State == EntityState.Added)
                {
                    var date = entity.Timestamp.ToUnixDate();
                    var hourStamp = new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0).ToUnixTimestamp();
                    var dayStamp = new DateTime(date.Year, date.Month, date.Day).ToUnixTimestamp();
                    var monthStamp = new DateTime(date.Year, date.Month, 1).ToUnixTimestamp();

                    entity.HourlyStamp = hourStamp;
                    entity.DailyStamp = dayStamp;
                    entity.MonthlyStamp = monthStamp;
                }
            }
        }
    }
}
