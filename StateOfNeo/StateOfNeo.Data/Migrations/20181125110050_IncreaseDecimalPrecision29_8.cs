using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class IncreaseDecimalPrecision29_8 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "SystemFee",
                table: "Transactions",
                type: "decimal(29, 8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(26, 9)");

            migrationBuilder.AlterColumn<decimal>(
                name: "NetworkFee",
                table: "Transactions",
                type: "decimal(29, 8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(26, 9)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "TransactedAssets",
                type: "decimal(29, 8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(26, 9)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "RegisterTransactions",
                type: "decimal(29, 8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(26, 9)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Peers",
                table: "NodeAudits",
                type: "decimal(29, 8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(26, 9)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Gas",
                table: "InvocationTransactions",
                type: "decimal(29, 8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(26, 9)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "AddressesInTransactions",
                type: "decimal(29, 8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(26, 9)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "AddressesInAssetTransactions",
                type: "decimal(29, 8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(26, 9)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Balance",
                table: "AddressBalances",
                type: "decimal(29, 8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(26, 9)");

            migrationBuilder.CreateIndex(
                name: "IX_AssetsInTransactions_Timestamp_AssetHash",
                table: "AssetsInTransactions",
                columns: new[] { "Timestamp", "AssetHash" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AssetsInTransactions_Timestamp_AssetHash",
                table: "AssetsInTransactions");

            migrationBuilder.AlterColumn<decimal>(
                name: "SystemFee",
                table: "Transactions",
                type: "decimal(26, 9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(29, 8)");

            migrationBuilder.AlterColumn<decimal>(
                name: "NetworkFee",
                table: "Transactions",
                type: "decimal(26, 9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(29, 8)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "TransactedAssets",
                type: "decimal(26, 9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(29, 8)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "RegisterTransactions",
                type: "decimal(26, 9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(29, 8)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Peers",
                table: "NodeAudits",
                type: "decimal(26, 9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(29, 8)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Gas",
                table: "InvocationTransactions",
                type: "decimal(26, 9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(29, 8)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "AddressesInTransactions",
                type: "decimal(26, 9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(29, 8)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "AddressesInAssetTransactions",
                type: "decimal(26, 9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(29, 8)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Balance",
                table: "AddressBalances",
                type: "decimal(26, 9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(29, 8)");
        }
    }
}
