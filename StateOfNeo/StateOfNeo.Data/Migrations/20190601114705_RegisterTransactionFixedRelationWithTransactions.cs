using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class RegisterTransactionFixedRelationWithTransactions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "RegisterTransactions");

            migrationBuilder.AddColumn<string>(
                name: "TransactionHash",
                table: "RegisterTransactions",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransactionHash",
                table: "RegisterTransactions");

            migrationBuilder.AddColumn<int>(
                name: "TransactionId",
                table: "RegisterTransactions",
                nullable: false,
                defaultValue: 0);
        }
    }
}
