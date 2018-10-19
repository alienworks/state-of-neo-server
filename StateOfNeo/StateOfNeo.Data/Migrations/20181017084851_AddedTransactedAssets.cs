using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class AddedTransactedAssets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NodeStatusUpdate_Block_BlockHash",
                table: "NodeStatusUpdate");

            migrationBuilder.DropForeignKey(
                name: "FK_NodeStatusUpdate_Nodes_NodeId",
                table: "NodeStatusUpdate");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Block_BlockHash",
                table: "Transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Address_FromAddressPublicAddress",
                table: "Transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Address_ToAddressPublicAddress",
                table: "Transaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Transaction",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_FromAddressPublicAddress",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_ToAddressPublicAddress",
                table: "Transaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NodeStatusUpdate",
                table: "NodeStatusUpdate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Block",
                table: "Block");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Address",
                table: "Address");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "AssetType",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "FromAddressId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "FromAddressPublicAddress",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "ToAddressPublicAddress",
                table: "Transaction");

            migrationBuilder.RenameTable(
                name: "Transaction",
                newName: "Transactions");

            migrationBuilder.RenameTable(
                name: "NodeStatusUpdate",
                newName: "NodeStatusUpdates");

            migrationBuilder.RenameTable(
                name: "Block",
                newName: "Blocks");

            migrationBuilder.RenameTable(
                name: "Address",
                newName: "Addresses");

            migrationBuilder.RenameColumn(
                name: "ToAddressId",
                table: "Transactions",
                newName: "NetworkFee");

            migrationBuilder.RenameIndex(
                name: "IX_Transaction_BlockHash",
                table: "Transactions",
                newName: "IX_Transactions_BlockHash");

            migrationBuilder.RenameIndex(
                name: "IX_NodeStatusUpdate_NodeId",
                table: "NodeStatusUpdates",
                newName: "IX_NodeStatusUpdates_NodeId");

            migrationBuilder.RenameIndex(
                name: "IX_NodeStatusUpdate_BlockHash",
                table: "NodeStatusUpdates",
                newName: "IX_NodeStatusUpdates_BlockHash");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Transactions",
                table: "Transactions",
                column: "ScriptHash");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NodeStatusUpdates",
                table: "NodeStatusUpdates",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Blocks",
                table: "Blocks",
                column: "Hash");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Addresses",
                table: "Addresses",
                column: "PublicAddress");

            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false),
                    MaxSupply = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransactedAssets",
                columns: table => new
                {
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Amount = table.Column<decimal>(nullable: false),
                    AssetType = table.Column<int>(nullable: false),
                    FromAddressId = table.Column<int>(nullable: false),
                    FromAddressPublicAddress = table.Column<string>(nullable: true),
                    ToAddressId = table.Column<int>(nullable: false),
                    ToAddressPublicAddress = table.Column<string>(nullable: true),
                    TransactionId = table.Column<int>(nullable: false),
                    TransactionScriptHash = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactedAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactedAssets_Addresses_FromAddressPublicAddress",
                        column: x => x.FromAddressPublicAddress,
                        principalTable: "Addresses",
                        principalColumn: "PublicAddress",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactedAssets_Addresses_ToAddressPublicAddress",
                        column: x => x.ToAddressPublicAddress,
                        principalTable: "Addresses",
                        principalColumn: "PublicAddress",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactedAssets_Transactions_TransactionScriptHash",
                        column: x => x.TransactionScriptHash,
                        principalTable: "Transactions",
                        principalColumn: "ScriptHash",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransactedAssets_FromAddressPublicAddress",
                table: "TransactedAssets",
                column: "FromAddressPublicAddress");

            migrationBuilder.CreateIndex(
                name: "IX_TransactedAssets_ToAddressPublicAddress",
                table: "TransactedAssets",
                column: "ToAddressPublicAddress");

            migrationBuilder.CreateIndex(
                name: "IX_TransactedAssets_TransactionScriptHash",
                table: "TransactedAssets",
                column: "TransactionScriptHash");

            migrationBuilder.AddForeignKey(
                name: "FK_NodeStatusUpdates_Blocks_BlockHash",
                table: "NodeStatusUpdates",
                column: "BlockHash",
                principalTable: "Blocks",
                principalColumn: "Hash",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NodeStatusUpdates_Nodes_NodeId",
                table: "NodeStatusUpdates",
                column: "NodeId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Blocks_BlockHash",
                table: "Transactions",
                column: "BlockHash",
                principalTable: "Blocks",
                principalColumn: "Hash",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NodeStatusUpdates_Blocks_BlockHash",
                table: "NodeStatusUpdates");

            migrationBuilder.DropForeignKey(
                name: "FK_NodeStatusUpdates_Nodes_NodeId",
                table: "NodeStatusUpdates");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Blocks_BlockHash",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "Assets");

            migrationBuilder.DropTable(
                name: "TransactedAssets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Transactions",
                table: "Transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NodeStatusUpdates",
                table: "NodeStatusUpdates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Blocks",
                table: "Blocks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Addresses",
                table: "Addresses");

            migrationBuilder.RenameTable(
                name: "Transactions",
                newName: "Transaction");

            migrationBuilder.RenameTable(
                name: "NodeStatusUpdates",
                newName: "NodeStatusUpdate");

            migrationBuilder.RenameTable(
                name: "Blocks",
                newName: "Block");

            migrationBuilder.RenameTable(
                name: "Addresses",
                newName: "Address");

            migrationBuilder.RenameColumn(
                name: "NetworkFee",
                table: "Transaction",
                newName: "ToAddressId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_BlockHash",
                table: "Transaction",
                newName: "IX_Transaction_BlockHash");

            migrationBuilder.RenameIndex(
                name: "IX_NodeStatusUpdates_NodeId",
                table: "NodeStatusUpdate",
                newName: "IX_NodeStatusUpdate_NodeId");

            migrationBuilder.RenameIndex(
                name: "IX_NodeStatusUpdates_BlockHash",
                table: "NodeStatusUpdate",
                newName: "IX_NodeStatusUpdate_BlockHash");

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "Transaction",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "AssetType",
                table: "Transaction",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FromAddressId",
                table: "Transaction",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FromAddressPublicAddress",
                table: "Transaction",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToAddressPublicAddress",
                table: "Transaction",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Transaction",
                table: "Transaction",
                column: "ScriptHash");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NodeStatusUpdate",
                table: "NodeStatusUpdate",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Block",
                table: "Block",
                column: "Hash");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Address",
                table: "Address",
                column: "PublicAddress");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_FromAddressPublicAddress",
                table: "Transaction",
                column: "FromAddressPublicAddress");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_ToAddressPublicAddress",
                table: "Transaction",
                column: "ToAddressPublicAddress");

            migrationBuilder.AddForeignKey(
                name: "FK_NodeStatusUpdate_Block_BlockHash",
                table: "NodeStatusUpdate",
                column: "BlockHash",
                principalTable: "Block",
                principalColumn: "Hash",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NodeStatusUpdate_Nodes_NodeId",
                table: "NodeStatusUpdate",
                column: "NodeId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Block_BlockHash",
                table: "Transaction",
                column: "BlockHash",
                principalTable: "Block",
                principalColumn: "Hash",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Address_FromAddressPublicAddress",
                table: "Transaction",
                column: "FromAddressPublicAddress",
                principalTable: "Address",
                principalColumn: "PublicAddress",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Address_ToAddressPublicAddress",
                table: "Transaction",
                column: "ToAddressPublicAddress",
                principalTable: "Address",
                principalColumn: "PublicAddress",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
