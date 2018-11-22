using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class RemovedAssetInTransactionCompositeIndexAddTimestamp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AssetsInTransactions_AssetHash_TransactionHash",
                table: "AssetsInTransactions");

            migrationBuilder.AddColumn<long>(
                name: "Timestamp",
                table: "AssetsInTransactions",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_AssetsInTransactions_AssetHash",
                table: "AssetsInTransactions",
                column: "AssetHash");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AssetsInTransactions_AssetHash",
                table: "AssetsInTransactions");

            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "AssetsInTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_AssetsInTransactions_AssetHash_TransactionHash",
                table: "AssetsInTransactions",
                columns: new[] { "AssetHash", "TransactionHash" });
        }
    }
}
