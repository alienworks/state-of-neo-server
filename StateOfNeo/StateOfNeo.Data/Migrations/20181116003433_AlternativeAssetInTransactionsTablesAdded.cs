using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class AlternativeAssetInTransactionsTablesAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssetsInTransactions",
                columns: table => new
                {
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TransactionHash = table.Column<string>(nullable: true),
                    AssetHash = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetsInTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetsInTransactions_Assets_AssetHash",
                        column: x => x.AssetHash,
                        principalTable: "Assets",
                        principalColumn: "Hash",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetsInTransactions_Transactions_TransactionHash",
                        column: x => x.TransactionHash,
                        principalTable: "Transactions",
                        principalColumn: "Hash",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AddressesInAssetTransactions",
                columns: table => new
                {
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AssetInTransactionId = table.Column<int>(nullable: false),
                    AddressPublicAddress = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressesInAssetTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AddressesInAssetTransactions_Addresses_AddressPublicAddress",
                        column: x => x.AddressPublicAddress,
                        principalTable: "Addresses",
                        principalColumn: "PublicAddress",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AddressesInAssetTransactions_AssetsInTransactions_AssetInTransactionId",
                        column: x => x.AssetInTransactionId,
                        principalTable: "AssetsInTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddressesInAssetTransactions_AddressPublicAddress",
                table: "AddressesInAssetTransactions",
                column: "AddressPublicAddress");

            migrationBuilder.CreateIndex(
                name: "IX_AddressesInAssetTransactions_AssetInTransactionId",
                table: "AddressesInAssetTransactions",
                column: "AssetInTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetsInTransactions_AssetHash",
                table: "AssetsInTransactions",
                column: "AssetHash");

            migrationBuilder.CreateIndex(
                name: "IX_AssetsInTransactions_TransactionHash",
                table: "AssetsInTransactions",
                column: "TransactionHash");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddressesInAssetTransactions");

            migrationBuilder.DropTable(
                name: "AssetsInTransactions");
        }
    }
}
