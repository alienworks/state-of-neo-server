using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class AddConsensusNodesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConsensusNodes",
                columns: table => new
                {
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Organization = table.Column<string>(nullable: true),
                    Logo = table.Column<string>(nullable: true),
                    PublicKey = table.Column<string>(nullable: true),
                    Address = table.Column<string>(nullable: true),
                    PublicKeyHash = table.Column<string>(nullable: true),
                    CollectedFees = table.Column<decimal>(type: "decimal(36, 8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsensusNodes", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsensusNodes");
        }
    }
}
