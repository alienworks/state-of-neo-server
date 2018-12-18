using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class TotalStats_ForgotenSetOfPropertiesAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "TransactionsCount",
                table: "TotalStats",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<long>(
                name: "Timestamp",
                table: "TotalStats",
                nullable: true,
                oldClrType: typeof(long));

            migrationBuilder.AlterColumn<decimal>(
                name: "ClaimedGas",
                table: "TotalStats",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(36, 8)");

            migrationBuilder.AlterColumn<int>(
                name: "BlockCount",
                table: "TotalStats",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "AddressCount",
                table: "TotalStats",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssetsCount",
                table: "TotalStats",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "BlocksSizes",
                table: "TotalStats",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BlocksTimes",
                table: "TotalStats",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressCount",
                table: "TotalStats");

            migrationBuilder.DropColumn(
                name: "AssetsCount",
                table: "TotalStats");

            migrationBuilder.DropColumn(
                name: "BlocksSizes",
                table: "TotalStats");

            migrationBuilder.DropColumn(
                name: "BlocksTimes",
                table: "TotalStats");

            migrationBuilder.AlterColumn<int>(
                name: "TransactionsCount",
                table: "TotalStats",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Timestamp",
                table: "TotalStats",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ClaimedGas",
                table: "TotalStats",
                type: "decimal(36, 8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BlockCount",
                table: "TotalStats",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
