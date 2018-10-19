using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class AddSystemFee : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "NetworkFee",
                table: "Transactions",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "Size",
                table: "Transactions",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "SystemFee",
                table: "Transactions",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Size",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "SystemFee",
                table: "Transactions");

            migrationBuilder.AlterColumn<int>(
                name: "NetworkFee",
                table: "Transactions",
                nullable: false,
                oldClrType: typeof(decimal));
        }
    }
}
