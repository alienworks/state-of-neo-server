using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class RenamedTransactedAssetAddressesColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FromAddressId",
                table: "TransactedAssets");

            migrationBuilder.DropColumn(
                name: "ToAddressId",
                table: "TransactedAssets");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FromAddressId",
                table: "TransactedAssets",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ToAddressId",
                table: "TransactedAssets",
                nullable: false,
                defaultValue: 0);
        }
    }
}
