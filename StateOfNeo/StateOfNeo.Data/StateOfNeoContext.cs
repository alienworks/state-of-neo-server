﻿using Microsoft.EntityFrameworkCore;
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

        public static StateOfNeoContext Create(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<StateOfNeoContext>();
            optionsBuilder.UseSqlServer(connectionString, opts => opts.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds));
            return new StateOfNeoContext(optionsBuilder.Options);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Address>().HasIndex(x => x.LastTransactionOn);

            modelBuilder.Entity<Block>().HasIndex(x => x.Timestamp);
            modelBuilder.Entity<Block>().HasIndex(x => x.Height);

            modelBuilder.Entity<Transaction>().HasIndex(x => x.Timestamp);

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
                property.Relational().ColumnType = "decimal(26, 9)";
            }
        }
    }
}
