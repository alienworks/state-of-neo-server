using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class AddSmartContracts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TimeStamp",
                table: "ChartEntries",
                newName: "Timestamp");

            migrationBuilder.AddColumn<long>(
                name: "LastTransactionStamp",
                table: "Addresses",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<decimal>(
                name: "Balance",
                table: "AddressBalances",
                type: "decimal(36, 8)",
                nullable: false,
                oldClrType: typeof(float));

            migrationBuilder.CreateTable(
                name: "SmartContracts",
                columns: table => new
                {
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Hash = table.Column<string>(nullable: true),
                    Timestamp = table.Column<long>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Author = table.Column<string>(nullable: true),
                    Version = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    HasStorage = table.Column<bool>(nullable: false),
                    Payable = table.Column<bool>(nullable: false),
                    HasDynamicInvoke = table.Column<bool>(nullable: false),
                    InputParameters = table.Column<string>(nullable: true),
                    ReturnType = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmartContracts", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SmartContracts");

            migrationBuilder.DropColumn(
                name: "LastTransactionStamp",
                table: "Addresses");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "ChartEntries",
                newName: "TimeStamp");

            migrationBuilder.AlterColumn<float>(
                name: "Balance",
                table: "AddressBalances",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(36, 8)");
        }
    }
}
