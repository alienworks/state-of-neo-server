using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class AddedAddressAssetBalance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AddressAssetBalance",
                columns: table => new
                {
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Balance = table.Column<decimal>(nullable: false),
                    AddressPublicAddress = table.Column<string>(nullable: true),
                    AssetId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressAssetBalance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AddressAssetBalance_Addresses_AddressPublicAddress",
                        column: x => x.AddressPublicAddress,
                        principalTable: "Addresses",
                        principalColumn: "PublicAddress",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AddressAssetBalance_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddressAssetBalance_AddressPublicAddress",
                table: "AddressAssetBalance",
                column: "AddressPublicAddress");

            migrationBuilder.CreateIndex(
                name: "IX_AddressAssetBalance_AssetId",
                table: "AddressAssetBalance",
                column: "AssetId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddressAssetBalance");
        }
    }
}
