using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class AddContractRelationsToInvocationTransactions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContractHash",
                table: "InvocationTransactions",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SmartContractId",
                table: "InvocationTransactions",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvocationTransactions_SmartContractId",
                table: "InvocationTransactions",
                column: "SmartContractId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvocationTransactions_SmartContracts_SmartContractId",
                table: "InvocationTransactions",
                column: "SmartContractId",
                principalTable: "SmartContracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvocationTransactions_SmartContracts_SmartContractId",
                table: "InvocationTransactions");

            migrationBuilder.DropIndex(
                name: "IX_InvocationTransactions_SmartContractId",
                table: "InvocationTransactions");

            migrationBuilder.DropColumn(
                name: "ContractHash",
                table: "InvocationTransactions");

            migrationBuilder.DropColumn(
                name: "SmartContractId",
                table: "InvocationTransactions");
        }
    }
}
