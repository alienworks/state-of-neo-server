using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class DecimalsPrecisionMovedTo36_8 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "SystemFee",
                table: "Transactions",
                type: "decimal(36, 8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(29, 8)");

            migrationBuilder.AlterColumn<decimal>(
                name: "NetworkFee",
                table: "Transactions",
                type: "decimal(36, 8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(29, 8)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "TransactedAssets",
                type: "decimal(36, 8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(29, 8)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "RegisterTransactions",
                type: "decimal(36, 8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(29, 8)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Peers",
                table: "NodeAudits",
                type: "decimal(36, 8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(29, 8)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Gas",
                table: "InvocationTransactions",
                type: "decimal(36, 8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(29, 8)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "AddressesInTransactions",
                type: "decimal(36, 8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(29, 8)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "AddressesInAssetTransactions",
                type: "decimal(36, 8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(29, 8)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Balance",
                table: "AddressBalances",
                type: "decimal(36, 8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(29, 8)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "SystemFee",
                table: "Transactions",
                type: "decimal(29, 8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(36, 8)");

            migrationBuilder.AlterColumn<decimal>(
                name: "NetworkFee",
                table: "Transactions",
                type: "decimal(29, 8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(36, 8)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "TransactedAssets",
                type: "decimal(29, 8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(36, 8)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "RegisterTransactions",
                type: "decimal(29, 8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(36, 8)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Peers",
                table: "NodeAudits",
                type: "decimal(29, 8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(36, 8)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Gas",
                table: "InvocationTransactions",
                type: "decimal(29, 8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(36, 8)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "AddressesInTransactions",
                type: "decimal(29, 8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(36, 8)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "AddressesInAssetTransactions",
                type: "decimal(29, 8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(36, 8)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Balance",
                table: "AddressBalances",
                type: "decimal(29, 8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(36, 8)");
        }
    }
}
