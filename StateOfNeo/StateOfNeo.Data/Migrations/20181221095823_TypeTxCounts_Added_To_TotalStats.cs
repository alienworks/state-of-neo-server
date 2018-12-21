using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class TypeTxCounts_Added_To_TotalStats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "NeoGasTxCount",
                table: "TotalStats",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Nep5TxCount",
                table: "TotalStats",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NeoGasTxCount",
                table: "TotalStats");

            migrationBuilder.DropColumn(
                name: "Nep5TxCount",
                table: "TotalStats");
        }
    }
}
