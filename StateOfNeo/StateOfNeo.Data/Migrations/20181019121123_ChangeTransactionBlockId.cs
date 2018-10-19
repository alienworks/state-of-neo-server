using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class ChangeTransactionBlockId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Blocks_BlockHash",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_BlockHash",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "BlockHash",
                table: "Transactions");

            migrationBuilder.AlterColumn<string>(
                name: "BlockId",
                table: "Transactions",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_BlockId",
                table: "Transactions",
                column: "BlockId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Blocks_BlockId",
                table: "Transactions",
                column: "BlockId",
                principalTable: "Blocks",
                principalColumn: "Hash",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Blocks_BlockId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_BlockId",
                table: "Transactions");

            migrationBuilder.AlterColumn<int>(
                name: "BlockId",
                table: "Transactions",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BlockHash",
                table: "Transactions",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_BlockHash",
                table: "Transactions",
                column: "BlockHash");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Blocks_BlockHash",
                table: "Transactions",
                column: "BlockHash",
                principalTable: "Blocks",
                principalColumn: "Hash",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
