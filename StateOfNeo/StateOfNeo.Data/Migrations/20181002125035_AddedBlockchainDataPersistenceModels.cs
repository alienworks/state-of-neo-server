using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class AddedBlockchainDataPersistenceModels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlockchainInfos");

            migrationBuilder.DropTable(
                name: "MainNetBlockInfos");

            migrationBuilder.DropTable(
                name: "TestNetBlockInfos");

            migrationBuilder.DropTable(
                name: "TimeEvents");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "Nodes",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsHttps",
                table: "Nodes",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Address",
                columns: table => new
                {
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    PublicAddress = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address", x => x.PublicAddress);
                });

            migrationBuilder.CreateTable(
                name: "Block",
                columns: table => new
                {
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Hash = table.Column<string>(nullable: false),
                    Height = table.Column<int>(nullable: false),
                    Timestamp = table.Column<long>(nullable: false),
                    Size = table.Column<int>(nullable: false),
                    Validator = table.Column<string>(nullable: true),
                    InvocationScript = table.Column<string>(nullable: true),
                    VerificationScript = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Block", x => x.Hash);
                });

            migrationBuilder.CreateTable(
                name: "NodeStatusUpdate",
                columns: table => new
                {
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsRpcOnline = table.Column<bool>(nullable: false),
                    IsP2pTcpOnline = table.Column<bool>(nullable: false),
                    IsP2pWsOnline = table.Column<bool>(nullable: false),
                    BlockId = table.Column<int>(nullable: false),
                    BlockHash = table.Column<string>(nullable: true),
                    NodeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodeStatusUpdate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NodeStatusUpdate_Block_BlockHash",
                        column: x => x.BlockHash,
                        principalTable: "Block",
                        principalColumn: "Hash",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NodeStatusUpdate_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transaction",
                columns: table => new
                {
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    ScriptHash = table.Column<string>(nullable: false),
                    Amount = table.Column<decimal>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    AssetType = table.Column<int>(nullable: false),
                    FromAddressId = table.Column<int>(nullable: false),
                    FromAddressPublicAddress = table.Column<string>(nullable: true),
                    ToAddressId = table.Column<int>(nullable: false),
                    ToAddressPublicAddress = table.Column<string>(nullable: true),
                    BlockId = table.Column<int>(nullable: false),
                    BlockHash = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaction", x => x.ScriptHash);
                    table.ForeignKey(
                        name: "FK_Transaction_Block_BlockHash",
                        column: x => x.BlockHash,
                        principalTable: "Block",
                        principalColumn: "Hash",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transaction_Address_FromAddressPublicAddress",
                        column: x => x.FromAddressPublicAddress,
                        principalTable: "Address",
                        principalColumn: "PublicAddress",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transaction_Address_ToAddressPublicAddress",
                        column: x => x.ToAddressPublicAddress,
                        principalTable: "Address",
                        principalColumn: "PublicAddress",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NodeStatusUpdate_BlockHash",
                table: "NodeStatusUpdate",
                column: "BlockHash");

            migrationBuilder.CreateIndex(
                name: "IX_NodeStatusUpdate_NodeId",
                table: "NodeStatusUpdate",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_BlockHash",
                table: "Transaction",
                column: "BlockHash");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_FromAddressPublicAddress",
                table: "Transaction",
                column: "FromAddressPublicAddress");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_ToAddressPublicAddress",
                table: "Transaction",
                column: "ToAddressPublicAddress");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NodeStatusUpdate");

            migrationBuilder.DropTable(
                name: "Transaction");

            migrationBuilder.DropTable(
                name: "Block");

            migrationBuilder.DropTable(
                name: "Address");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "IsHttps",
                table: "Nodes");

            migrationBuilder.CreateTable(
                name: "BlockchainInfos",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BlockCount = table.Column<long>(nullable: false),
                    Net = table.Column<string>(nullable: true),
                    SecondsCount = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockchainInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MainNetBlockInfos",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BlockHeight = table.Column<decimal>(nullable: false),
                    SecondsCount = table.Column<int>(nullable: false),
                    TxCount = table.Column<long>(nullable: false),
                    TxNetworkFees = table.Column<long>(nullable: false),
                    TxOutputValues = table.Column<long>(nullable: false),
                    TxSystemFees = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MainNetBlockInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TestNetBlockInfos",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BlockHeight = table.Column<decimal>(nullable: false),
                    SecondsCount = table.Column<int>(nullable: false),
                    TxCount = table.Column<long>(nullable: false),
                    TxNetworkFees = table.Column<long>(nullable: false),
                    TxOutputValues = table.Column<long>(nullable: false),
                    TxSystemFees = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestNetBlockInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TimeEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    LastDownTime = table.Column<DateTime>(nullable: true),
                    NodeId = table.Column<int>(nullable: true),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeEvents_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TimeEvents_NodeId",
                table: "TimeEvents",
                column: "NodeId");
        }
    }
}
