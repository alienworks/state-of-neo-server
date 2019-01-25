using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class AddedNodeToPeerConnection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NodeId",
                table: "Peers",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Peers_NodeId",
                table: "Peers",
                column: "NodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Peers_Nodes_NodeId",
                table: "Peers",
                column: "NodeId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Peers_Nodes_NodeId",
                table: "Peers");

            migrationBuilder.DropIndex(
                name: "IX_Peers_NodeId",
                table: "Peers");

            migrationBuilder.DropColumn(
                name: "NodeId",
                table: "Peers");
        }
    }
}
