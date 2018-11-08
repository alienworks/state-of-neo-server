using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class IncreaseDecimalPrecision : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "SystemFee",
                table: "Transactions",
                type: "decimal(26, 9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(20, 9)");

            migrationBuilder.AlterColumn<decimal>(
                name: "NetworkFee",
                table: "Transactions",
                type: "decimal(26, 9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(20, 9)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "TransactedAssets",
                type: "decimal(26, 9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(20, 9)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "RegisterTransactions",
                type: "decimal(26, 9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(20, 9)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Gas",
                table: "InvocationTransactions",
                type: "decimal(26, 9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(20, 9)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Balance",
                table: "AddressBalances",
                type: "decimal(26, 9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(20, 9)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "SystemFee",
                table: "Transactions",
                type: "decimal(20, 9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(26, 9)");

            migrationBuilder.AlterColumn<decimal>(
                name: "NetworkFee",
                table: "Transactions",
                type: "decimal(20, 9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(26, 9)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "TransactedAssets",
                type: "decimal(20, 9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(26, 9)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "RegisterTransactions",
                type: "decimal(20, 9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(26, 9)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Gas",
                table: "InvocationTransactions",
                type: "decimal(20, 9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(26, 9)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Balance",
                table: "AddressBalances",
                type: "decimal(20, 9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(26, 9)");
        }
    }
}
