using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class NodeTimesAddedInNode_NodeAuditAdd_NodeStatusRenamed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "FirstRuntime",
                table: "Nodes",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LastAudit",
                table: "Nodes",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LatestRuntime",
                table: "Nodes",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SecondsOnline",
                table: "Nodes",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "NodeAudits",
                columns: table => new
                {
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Timestamp = table.Column<long>(nullable: false),
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Latency = table.Column<int>(nullable: false),
                    Peers = table.Column<decimal>(type: "decimal(26, 9)", nullable: false),
                    NodeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodeAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NodeAudits_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NodeAudits_NodeId",
                table: "NodeAudits",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeAudits_Timestamp",
                table: "NodeAudits",
                column: "Timestamp");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NodeAudits");

            migrationBuilder.DropColumn(
                name: "FirstRuntime",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "LastAudit",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "LatestRuntime",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "SecondsOnline",
                table: "Nodes");
        }
    }
}
