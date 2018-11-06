using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class ReworkedTransactedAssets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InGlobalTransactionScriptHash",
                table: "TransactedAssets",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OutGlobalTransactionScriptHash",
                table: "TransactedAssets",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactedAssets_InGlobalTransactionScriptHash",
                table: "TransactedAssets",
                column: "InGlobalTransactionScriptHash");

            migrationBuilder.CreateIndex(
                name: "IX_TransactedAssets_OutGlobalTransactionScriptHash",
                table: "TransactedAssets",
                column: "OutGlobalTransactionScriptHash");

            migrationBuilder.AddForeignKey(
                name: "FK_TransactedAssets_Transactions_InGlobalTransactionScriptHash",
                table: "TransactedAssets",
                column: "InGlobalTransactionScriptHash",
                principalTable: "Transactions",
                principalColumn: "ScriptHash",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactedAssets_Transactions_OutGlobalTransactionScriptHash",
                table: "TransactedAssets",
                column: "OutGlobalTransactionScriptHash",
                principalTable: "Transactions",
                principalColumn: "ScriptHash",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransactedAssets_Transactions_InGlobalTransactionScriptHash",
                table: "TransactedAssets");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactedAssets_Transactions_OutGlobalTransactionScriptHash",
                table: "TransactedAssets");

            migrationBuilder.DropIndex(
                name: "IX_TransactedAssets_InGlobalTransactionScriptHash",
                table: "TransactedAssets");

            migrationBuilder.DropIndex(
                name: "IX_TransactedAssets_OutGlobalTransactionScriptHash",
                table: "TransactedAssets");

            migrationBuilder.DropColumn(
                name: "InGlobalTransactionScriptHash",
                table: "TransactedAssets");

            migrationBuilder.DropColumn(
                name: "OutGlobalTransactionScriptHash",
                table: "TransactedAssets");
        }
    }
}
