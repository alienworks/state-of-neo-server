using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class AddedAddressInTransaction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AddressBalances_Assets_AssetId",
                table: "AddressBalances");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactedAssets_Assets_AssetId",
                table: "TransactedAssets");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactedAssets_Transactions_InGlobalTransactionScriptHash",
                table: "TransactedAssets");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactedAssets_Transactions_OutGlobalTransactionScriptHash",
                table: "TransactedAssets");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactedAssets_Transactions_TransactionScriptHash",
                table: "TransactedAssets");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactionAttributes_Transactions_TransactionScriptHash",
                table: "TransactionAttributes");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactionWitnesses_Transactions_TransactionScriptHash",
                table: "TransactionWitnesses");

            migrationBuilder.DropIndex(
                name: "IX_TransactedAssets_AssetId",
                table: "TransactedAssets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Assets",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_AddressBalances_AssetId",
                table: "AddressBalances");

            migrationBuilder.DropColumn(
                name: "AssetId",
                table: "TransactedAssets");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Assets");

            migrationBuilder.RenameColumn(
                name: "TransactionScriptHash",
                table: "TransactionWitnesses",
                newName: "TransactionHash");

            migrationBuilder.RenameIndex(
                name: "IX_TransactionWitnesses_TransactionScriptHash",
                table: "TransactionWitnesses",
                newName: "IX_TransactionWitnesses_TransactionHash");

            migrationBuilder.RenameColumn(
                name: "ScriptHash",
                table: "Transactions",
                newName: "Hash");

            migrationBuilder.RenameColumn(
                name: "TransactionScriptHash",
                table: "TransactionAttributes",
                newName: "TransactionHash");

            migrationBuilder.RenameIndex(
                name: "IX_TransactionAttributes_TransactionScriptHash",
                table: "TransactionAttributes",
                newName: "IX_TransactionAttributes_TransactionHash");

            migrationBuilder.RenameColumn(
                name: "TransactionScriptHash",
                table: "TransactedAssets",
                newName: "TransactionHash");

            migrationBuilder.RenameColumn(
                name: "OutGlobalTransactionScriptHash",
                table: "TransactedAssets",
                newName: "OutGlobalTransactionHash");

            migrationBuilder.RenameColumn(
                name: "InGlobalTransactionScriptHash",
                table: "TransactedAssets",
                newName: "InGlobalTransactionHash");

            migrationBuilder.RenameIndex(
                name: "IX_TransactedAssets_TransactionScriptHash",
                table: "TransactedAssets",
                newName: "IX_TransactedAssets_TransactionHash");

            migrationBuilder.RenameIndex(
                name: "IX_TransactedAssets_OutGlobalTransactionScriptHash",
                table: "TransactedAssets",
                newName: "IX_TransactedAssets_OutGlobalTransactionHash");

            migrationBuilder.RenameIndex(
                name: "IX_TransactedAssets_InGlobalTransactionScriptHash",
                table: "TransactedAssets",
                newName: "IX_TransactedAssets_InGlobalTransactionHash");

            migrationBuilder.RenameColumn(
                name: "AssetId",
                table: "AddressBalances",
                newName: "TransactionsCount");

            migrationBuilder.AddColumn<string>(
                name: "AssetHash",
                table: "TransactedAssets",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Hash",
                table: "Assets",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransactionsCount",
                table: "Assets",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TransactionsCount",
                table: "Addresses",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AssetHash",
                table: "AddressBalances",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Assets",
                table: "Assets",
                column: "Hash");

            migrationBuilder.CreateTable(
                name: "AddressesInTransactions",
                columns: table => new
                {
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Timestamp = table.Column<long>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Amount = table.Column<decimal>(type: "decimal(26, 9)", nullable: false),
                    AddressPublicAddress = table.Column<string>(nullable: true),
                    TransactionHash = table.Column<string>(nullable: true),
                    AssetHash = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressesInTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AddressesInTransactions_Addresses_AddressPublicAddress",
                        column: x => x.AddressPublicAddress,
                        principalTable: "Addresses",
                        principalColumn: "PublicAddress",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AddressesInTransactions_Assets_AssetHash",
                        column: x => x.AssetHash,
                        principalTable: "Assets",
                        principalColumn: "Hash",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AddressesInTransactions_Transactions_TransactionHash",
                        column: x => x.TransactionHash,
                        principalTable: "Transactions",
                        principalColumn: "Hash",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransactedAssets_AssetHash",
                table: "TransactedAssets",
                column: "AssetHash");

            migrationBuilder.CreateIndex(
                name: "IX_AddressBalances_AssetHash",
                table: "AddressBalances",
                column: "AssetHash");

            migrationBuilder.CreateIndex(
                name: "IX_AddressesInTransactions_AddressPublicAddress",
                table: "AddressesInTransactions",
                column: "AddressPublicAddress");

            migrationBuilder.CreateIndex(
                name: "IX_AddressesInTransactions_AssetHash",
                table: "AddressesInTransactions",
                column: "AssetHash");

            migrationBuilder.CreateIndex(
                name: "IX_AddressesInTransactions_TransactionHash",
                table: "AddressesInTransactions",
                column: "TransactionHash");

            migrationBuilder.AddForeignKey(
                name: "FK_AddressBalances_Assets_AssetHash",
                table: "AddressBalances",
                column: "AssetHash",
                principalTable: "Assets",
                principalColumn: "Hash",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactedAssets_Assets_AssetHash",
                table: "TransactedAssets",
                column: "AssetHash",
                principalTable: "Assets",
                principalColumn: "Hash",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactedAssets_Transactions_InGlobalTransactionHash",
                table: "TransactedAssets",
                column: "InGlobalTransactionHash",
                principalTable: "Transactions",
                principalColumn: "Hash",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactedAssets_Transactions_OutGlobalTransactionHash",
                table: "TransactedAssets",
                column: "OutGlobalTransactionHash",
                principalTable: "Transactions",
                principalColumn: "Hash",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactedAssets_Transactions_TransactionHash",
                table: "TransactedAssets",
                column: "TransactionHash",
                principalTable: "Transactions",
                principalColumn: "Hash",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionAttributes_Transactions_TransactionHash",
                table: "TransactionAttributes",
                column: "TransactionHash",
                principalTable: "Transactions",
                principalColumn: "Hash",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionWitnesses_Transactions_TransactionHash",
                table: "TransactionWitnesses",
                column: "TransactionHash",
                principalTable: "Transactions",
                principalColumn: "Hash",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AddressBalances_Assets_AssetHash",
                table: "AddressBalances");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactedAssets_Assets_AssetHash",
                table: "TransactedAssets");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactedAssets_Transactions_InGlobalTransactionHash",
                table: "TransactedAssets");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactedAssets_Transactions_OutGlobalTransactionHash",
                table: "TransactedAssets");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactedAssets_Transactions_TransactionHash",
                table: "TransactedAssets");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactionAttributes_Transactions_TransactionHash",
                table: "TransactionAttributes");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactionWitnesses_Transactions_TransactionHash",
                table: "TransactionWitnesses");

            migrationBuilder.DropTable(
                name: "AddressesInTransactions");

            migrationBuilder.DropIndex(
                name: "IX_TransactedAssets_AssetHash",
                table: "TransactedAssets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Assets",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_AddressBalances_AssetHash",
                table: "AddressBalances");

            migrationBuilder.DropColumn(
                name: "AssetHash",
                table: "TransactedAssets");

            migrationBuilder.DropColumn(
                name: "TransactionsCount",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "TransactionsCount",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "AssetHash",
                table: "AddressBalances");

            migrationBuilder.RenameColumn(
                name: "TransactionHash",
                table: "TransactionWitnesses",
                newName: "TransactionScriptHash");

            migrationBuilder.RenameIndex(
                name: "IX_TransactionWitnesses_TransactionHash",
                table: "TransactionWitnesses",
                newName: "IX_TransactionWitnesses_TransactionScriptHash");

            migrationBuilder.RenameColumn(
                name: "Hash",
                table: "Transactions",
                newName: "ScriptHash");

            migrationBuilder.RenameColumn(
                name: "TransactionHash",
                table: "TransactionAttributes",
                newName: "TransactionScriptHash");

            migrationBuilder.RenameIndex(
                name: "IX_TransactionAttributes_TransactionHash",
                table: "TransactionAttributes",
                newName: "IX_TransactionAttributes_TransactionScriptHash");

            migrationBuilder.RenameColumn(
                name: "TransactionHash",
                table: "TransactedAssets",
                newName: "TransactionScriptHash");

            migrationBuilder.RenameColumn(
                name: "OutGlobalTransactionHash",
                table: "TransactedAssets",
                newName: "OutGlobalTransactionScriptHash");

            migrationBuilder.RenameColumn(
                name: "InGlobalTransactionHash",
                table: "TransactedAssets",
                newName: "InGlobalTransactionScriptHash");

            migrationBuilder.RenameIndex(
                name: "IX_TransactedAssets_TransactionHash",
                table: "TransactedAssets",
                newName: "IX_TransactedAssets_TransactionScriptHash");

            migrationBuilder.RenameIndex(
                name: "IX_TransactedAssets_OutGlobalTransactionHash",
                table: "TransactedAssets",
                newName: "IX_TransactedAssets_OutGlobalTransactionScriptHash");

            migrationBuilder.RenameIndex(
                name: "IX_TransactedAssets_InGlobalTransactionHash",
                table: "TransactedAssets",
                newName: "IX_TransactedAssets_InGlobalTransactionScriptHash");

            migrationBuilder.RenameColumn(
                name: "TransactionsCount",
                table: "AddressBalances",
                newName: "AssetId");

            migrationBuilder.AddColumn<int>(
                name: "AssetId",
                table: "TransactedAssets",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Hash",
                table: "Assets",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Assets",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Assets",
                table: "Assets",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_TransactedAssets_AssetId",
                table: "TransactedAssets",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressBalances_AssetId",
                table: "AddressBalances",
                column: "AssetId");

            migrationBuilder.AddForeignKey(
                name: "FK_AddressBalances_Assets_AssetId",
                table: "AddressBalances",
                column: "AssetId",
                principalTable: "Assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactedAssets_Assets_AssetId",
                table: "TransactedAssets",
                column: "AssetId",
                principalTable: "Assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactedAssets_Transactions_InGlobalTransactionScriptHash",
                table: "TransactedAssets",
                column: "InGlobalTransactionScriptHash",
                principalTable: "Transactions",
                principalColumn: "ScriptHash",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactedAssets_Transactions_OutGlobalTransactionScriptHash",
                table: "TransactedAssets",
                column: "OutGlobalTransactionScriptHash",
                principalTable: "Transactions",
                principalColumn: "ScriptHash",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactedAssets_Transactions_TransactionScriptHash",
                table: "TransactedAssets",
                column: "TransactionScriptHash",
                principalTable: "Transactions",
                principalColumn: "ScriptHash",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionAttributes_Transactions_TransactionScriptHash",
                table: "TransactionAttributes",
                column: "TransactionScriptHash",
                principalTable: "Transactions",
                principalColumn: "ScriptHash",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionWitnesses_Transactions_TransactionScriptHash",
                table: "TransactionWitnesses",
                column: "TransactionScriptHash",
                principalTable: "Transactions",
                principalColumn: "ScriptHash",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
