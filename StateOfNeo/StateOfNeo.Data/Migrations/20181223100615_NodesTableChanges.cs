using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class NodesTableChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MemoryPool",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "Peers",
                table: "Nodes");

            migrationBuilder.AddColumn<string>(
                name: "Service",
                table: "Nodes",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Service",
                table: "Nodes");

            migrationBuilder.AddColumn<int>(
                name: "MemoryPool",
                table: "Nodes",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Peers",
                table: "Nodes",
                nullable: true);
        }
    }
}
