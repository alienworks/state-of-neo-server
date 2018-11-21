using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class AddStampedEntityGranularStamps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DailyStamp",
                table: "Transactions",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "HourlyStamp",
                table: "Transactions",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "MonthlyStamp",
                table: "Transactions",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "DailyStamp",
                table: "NodeAudits",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "HourlyStamp",
                table: "NodeAudits",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "MonthlyStamp",
                table: "NodeAudits",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "DailyStamp",
                table: "Blocks",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "HourlyStamp",
                table: "Blocks",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "MonthlyStamp",
                table: "Blocks",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "DailyStamp",
                table: "AddressesInTransactions",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "HourlyStamp",
                table: "AddressesInTransactions",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "MonthlyStamp",
                table: "AddressesInTransactions",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyStamp",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "HourlyStamp",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "MonthlyStamp",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "DailyStamp",
                table: "NodeAudits");

            migrationBuilder.DropColumn(
                name: "HourlyStamp",
                table: "NodeAudits");

            migrationBuilder.DropColumn(
                name: "MonthlyStamp",
                table: "NodeAudits");

            migrationBuilder.DropColumn(
                name: "DailyStamp",
                table: "Blocks");

            migrationBuilder.DropColumn(
                name: "HourlyStamp",
                table: "Blocks");

            migrationBuilder.DropColumn(
                name: "MonthlyStamp",
                table: "Blocks");

            migrationBuilder.DropColumn(
                name: "DailyStamp",
                table: "AddressesInTransactions");

            migrationBuilder.DropColumn(
                name: "HourlyStamp",
                table: "AddressesInTransactions");

            migrationBuilder.DropColumn(
                name: "MonthlyStamp",
                table: "AddressesInTransactions");
        }
    }
}
