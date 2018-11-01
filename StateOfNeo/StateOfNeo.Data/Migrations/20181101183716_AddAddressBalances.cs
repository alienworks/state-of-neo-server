using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class AddAddressBalances : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AddressAssetBalance_Addresses_AddressPublicAddress",
                table: "AddressAssetBalance");

            migrationBuilder.DropForeignKey(
                name: "FK_AddressAssetBalance_Assets_AssetId",
                table: "AddressAssetBalance");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AddressAssetBalance",
                table: "AddressAssetBalance");

            migrationBuilder.RenameTable(
                name: "AddressAssetBalance",
                newName: "AddressBalances");

            migrationBuilder.RenameIndex(
                name: "IX_AddressAssetBalance_AssetId",
                table: "AddressBalances",
                newName: "IX_AddressBalances_AssetId");

            migrationBuilder.RenameIndex(
                name: "IX_AddressAssetBalance_AddressPublicAddress",
                table: "AddressBalances",
                newName: "IX_AddressBalances_AddressPublicAddress");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AddressBalances",
                table: "AddressBalances",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AddressBalances_Addresses_AddressPublicAddress",
                table: "AddressBalances",
                column: "AddressPublicAddress",
                principalTable: "Addresses",
                principalColumn: "PublicAddress",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AddressBalances_Assets_AssetId",
                table: "AddressBalances",
                column: "AssetId",
                principalTable: "Assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AddressBalances_Addresses_AddressPublicAddress",
                table: "AddressBalances");

            migrationBuilder.DropForeignKey(
                name: "FK_AddressBalances_Assets_AssetId",
                table: "AddressBalances");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AddressBalances",
                table: "AddressBalances");

            migrationBuilder.RenameTable(
                name: "AddressBalances",
                newName: "AddressAssetBalance");

            migrationBuilder.RenameIndex(
                name: "IX_AddressBalances_AssetId",
                table: "AddressAssetBalance",
                newName: "IX_AddressAssetBalance_AssetId");

            migrationBuilder.RenameIndex(
                name: "IX_AddressBalances_AddressPublicAddress",
                table: "AddressAssetBalance",
                newName: "IX_AddressAssetBalance_AddressPublicAddress");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AddressAssetBalance",
                table: "AddressAssetBalance",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AddressAssetBalance_Addresses_AddressPublicAddress",
                table: "AddressAssetBalance",
                column: "AddressPublicAddress",
                principalTable: "Addresses",
                principalColumn: "PublicAddress",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AddressAssetBalance_Assets_AssetId",
                table: "AddressAssetBalance",
                column: "AssetId",
                principalTable: "Assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
