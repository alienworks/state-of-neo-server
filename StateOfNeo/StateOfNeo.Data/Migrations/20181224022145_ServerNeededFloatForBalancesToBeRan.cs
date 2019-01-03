using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class ServerNeededFloatForBalancesToBeRan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "Amount",
                table: "AddressesInTransactions",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(36, 8)");

            migrationBuilder.AlterColumn<float>(
                name: "Balance",
                table: "AddressBalances",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(36, 8)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "AddressesInTransactions",
                type: "decimal(36, 8)",
                nullable: false,
                oldClrType: typeof(float));

            migrationBuilder.AlterColumn<decimal>(
                name: "Balance",
                table: "AddressBalances",
                type: "decimal(36, 8)",
                nullable: false,
                oldClrType: typeof(float));
        }
    }
}
