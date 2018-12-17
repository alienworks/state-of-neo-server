using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class Add_ChartEntry_And_TotalStats_Tables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChartEntries",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<int>(nullable: false),
                    UnitOfTime = table.Column<int>(nullable: false),
                    TimeStamp = table.Column<long>(nullable: false),
                    Count = table.Column<long>(nullable: true),
                    Value = table.Column<decimal>(type: "decimal(36, 8)", nullable: false),
                    AccumulatedValue = table.Column<decimal>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChartEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TotalStats",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BlockCount = table.Column<int>(nullable: false),
                    ClaimedGas = table.Column<decimal>(type: "decimal(36, 8)", nullable: false),
                    TransactionsCount = table.Column<int>(nullable: false),
                    Timestamp = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TotalStats", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChartEntries");

            migrationBuilder.DropTable(
                name: "TotalStats");
        }
    }
}
