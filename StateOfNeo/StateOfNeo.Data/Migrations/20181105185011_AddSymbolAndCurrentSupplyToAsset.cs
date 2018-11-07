using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class AddSymbolAndCurrentSupplyToAsset : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "MaxSupply",
                table: "Assets",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<long>(
                name: "CurrentSupply",
                table: "Assets",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "Decimals",
                table: "Assets",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Symbol",
                table: "Assets",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentSupply",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Decimals",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Symbol",
                table: "Assets");

            migrationBuilder.AlterColumn<int>(
                name: "MaxSupply",
                table: "Assets",
                nullable: false,
                oldClrType: typeof(long));
        }
    }
}
