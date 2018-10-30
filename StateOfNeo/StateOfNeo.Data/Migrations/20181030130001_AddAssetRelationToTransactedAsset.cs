using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class AddAssetRelationToTransactedAsset : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssetId",
                table: "TransactedAssets",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Hash",
                table: "Assets",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactedAssets_AssetId",
                table: "TransactedAssets",
                column: "AssetId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransactedAssets_Assets_AssetId",
                table: "TransactedAssets",
                column: "AssetId",
                principalTable: "Assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransactedAssets_Assets_AssetId",
                table: "TransactedAssets");

            migrationBuilder.DropIndex(
                name: "IX_TransactedAssets_AssetId",
                table: "TransactedAssets");

            migrationBuilder.DropColumn(
                name: "AssetId",
                table: "TransactedAssets");

            migrationBuilder.DropColumn(
                name: "Hash",
                table: "Assets");
        }
    }
}
