using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class AddCompositeIndexForAssetsInTransaction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AssetsInTransactions_AssetHash",
                table: "AssetsInTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_AssetsInTransactions_AssetHash_TransactionHash",
                table: "AssetsInTransactions",
                columns: new[] { "AssetHash", "TransactionHash" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AssetsInTransactions_AssetHash_TransactionHash",
                table: "AssetsInTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_AssetsInTransactions_AssetHash",
                table: "AssetsInTransactions",
                column: "AssetHash");
        }
    }
}
