using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class AddressAssetBalance_BalanceToFloat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "Balance",
                table: "AddressBalances",
                type: "decimal(36, 8)",
                nullable: false,
                oldClrType: typeof(float));
        }
    }
}
