using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class AddCreatorAddressToAssets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatorAddressId",
                table: "Assets",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assets_CreatorAddressId",
                table: "Assets",
                column: "CreatorAddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Addresses_CreatorAddressId",
                table: "Assets",
                column: "CreatorAddressId",
                principalTable: "Addresses",
                principalColumn: "PublicAddress",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Addresses_CreatorAddressId",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_CreatorAddressId",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "CreatorAddressId",
                table: "Assets");
        }
    }
}
