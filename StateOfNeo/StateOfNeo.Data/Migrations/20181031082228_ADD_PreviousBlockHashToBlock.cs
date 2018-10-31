using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class ADD_PreviousBlockHashToBlock : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreviousBlockHash",
                table: "Blocks",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_PreviousBlockHash",
                table: "Blocks",
                column: "PreviousBlockHash");

            migrationBuilder.AddForeignKey(
                name: "FK_Blocks_Blocks_PreviousBlockHash",
                table: "Blocks",
                column: "PreviousBlockHash",
                principalTable: "Blocks",
                principalColumn: "Hash",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Blocks_Blocks_PreviousBlockHash",
                table: "Blocks");

            migrationBuilder.DropIndex(
                name: "IX_Blocks_PreviousBlockHash",
                table: "Blocks");

            migrationBuilder.DropColumn(
                name: "PreviousBlockHash",
                table: "Blocks");
        }
    }
}
