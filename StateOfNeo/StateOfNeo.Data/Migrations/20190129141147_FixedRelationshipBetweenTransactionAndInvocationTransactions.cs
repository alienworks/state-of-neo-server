using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class FixedRelationshipBetweenTransactionAndInvocationTransactions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "InvocationTransactions");

            migrationBuilder.AddColumn<string>(
                name: "TransactionHash",
                table: "InvocationTransactions",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransactionHash",
                table: "InvocationTransactions");

            migrationBuilder.AddColumn<int>(
                name: "TransactionId",
                table: "InvocationTransactions",
                nullable: false,
                defaultValue: 0);
        }
    }
}
