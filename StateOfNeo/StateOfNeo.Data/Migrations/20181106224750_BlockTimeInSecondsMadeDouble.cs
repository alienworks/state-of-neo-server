using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class BlockTimeInSecondsMadeDouble : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "TimeInSeconds",
                table: "Blocks",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(20, 9)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "TimeInSeconds",
                table: "Blocks",
                type: "decimal(20, 9)",
                nullable: false,
                oldClrType: typeof(double));
        }
    }
}
