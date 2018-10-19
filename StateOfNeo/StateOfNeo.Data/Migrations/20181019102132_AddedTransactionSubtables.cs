using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class AddedTransactionSubtables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte>(
                name: "Type",
                table: "Transactions",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "EnrollmentTransactionId",
                table: "Transactions",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvocationTransactionId",
                table: "Transactions",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinerTransactionId",
                table: "Transactions",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PublishTransactionId",
                table: "Transactions",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RegisterTransactionId",
                table: "Transactions",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StateTransactionId",
                table: "Transactions",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Transactions",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "EnrollmentTransactions",
                columns: table => new
                {
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PublicKey = table.Column<string>(nullable: true),
                    TransactionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnrollmentTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InvocationTransactions",
                columns: table => new
                {
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ScriptAsHexString = table.Column<string>(nullable: true),
                    Gas = table.Column<decimal>(nullable: false),
                    TransactionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvocationTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MinerTransaction",
                columns: table => new
                {
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Nonce = table.Column<long>(nullable: false),
                    TransactionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MinerTransaction", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PublishTransactions",
                columns: table => new
                {
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ScriptAsHexString = table.Column<string>(nullable: true),
                    ParameterList = table.Column<string>(nullable: true),
                    ReturnType = table.Column<string>(nullable: true),
                    NeedStorage = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    CodeVersion = table.Column<string>(nullable: true),
                    Author = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    TransactionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublishTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegisterTransactions",
                columns: table => new
                {
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AssetType = table.Column<byte>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Amount = table.Column<decimal>(nullable: false),
                    Precision = table.Column<byte>(nullable: false),
                    OwnerPublicKey = table.Column<string>(nullable: true),
                    AdminAddress = table.Column<string>(nullable: true),
                    TransactionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegisterTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StateTransactions",
                columns: table => new
                {
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TransactionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StateTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransactionAttributes",
                columns: table => new
                {
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Usage = table.Column<int>(nullable: false),
                    DataAsHexString = table.Column<string>(nullable: true),
                    TransactionId = table.Column<int>(nullable: false),
                    TransactionScriptHash = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionAttributes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionAttributes_Transactions_TransactionScriptHash",
                        column: x => x.TransactionScriptHash,
                        principalTable: "Transactions",
                        principalColumn: "ScriptHash",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransactionWitnesses",
                columns: table => new
                {
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    InvocationScriptAsHexString = table.Column<string>(nullable: true),
                    VerificationScriptAsHexString = table.Column<string>(nullable: true),
                    Address = table.Column<string>(nullable: true),
                    TransactionId = table.Column<int>(nullable: false),
                    TransactionScriptHash = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionWitnesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionWitnesses_Transactions_TransactionScriptHash",
                        column: x => x.TransactionScriptHash,
                        principalTable: "Transactions",
                        principalColumn: "ScriptHash",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StateDescriptors",
                columns: table => new
                {
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<byte>(nullable: false),
                    KeyAsHexString = table.Column<string>(nullable: true),
                    Field = table.Column<string>(nullable: true),
                    ValueAsHexString = table.Column<string>(nullable: true),
                    TransactionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StateDescriptors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StateDescriptors_StateTransactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "StateTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_EnrollmentTransactionId",
                table: "Transactions",
                column: "EnrollmentTransactionId",
                unique: true,
                filter: "[EnrollmentTransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_InvocationTransactionId",
                table: "Transactions",
                column: "InvocationTransactionId",
                unique: true,
                filter: "[InvocationTransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_MinerTransactionId",
                table: "Transactions",
                column: "MinerTransactionId",
                unique: true,
                filter: "[MinerTransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_PublishTransactionId",
                table: "Transactions",
                column: "PublishTransactionId",
                unique: true,
                filter: "[PublishTransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_RegisterTransactionId",
                table: "Transactions",
                column: "RegisterTransactionId",
                unique: true,
                filter: "[RegisterTransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_StateTransactionId",
                table: "Transactions",
                column: "StateTransactionId",
                unique: true,
                filter: "[StateTransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StateDescriptors_TransactionId",
                table: "StateDescriptors",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionAttributes_TransactionScriptHash",
                table: "TransactionAttributes",
                column: "TransactionScriptHash");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionWitnesses_TransactionScriptHash",
                table: "TransactionWitnesses",
                column: "TransactionScriptHash");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_EnrollmentTransactions_EnrollmentTransactionId",
                table: "Transactions",
                column: "EnrollmentTransactionId",
                principalTable: "EnrollmentTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_InvocationTransactions_InvocationTransactionId",
                table: "Transactions",
                column: "InvocationTransactionId",
                principalTable: "InvocationTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_MinerTransaction_MinerTransactionId",
                table: "Transactions",
                column: "MinerTransactionId",
                principalTable: "MinerTransaction",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_PublishTransactions_PublishTransactionId",
                table: "Transactions",
                column: "PublishTransactionId",
                principalTable: "PublishTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_RegisterTransactions_RegisterTransactionId",
                table: "Transactions",
                column: "RegisterTransactionId",
                principalTable: "RegisterTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_StateTransactions_StateTransactionId",
                table: "Transactions",
                column: "StateTransactionId",
                principalTable: "StateTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_EnrollmentTransactions_EnrollmentTransactionId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_InvocationTransactions_InvocationTransactionId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_MinerTransaction_MinerTransactionId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_PublishTransactions_PublishTransactionId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_RegisterTransactions_RegisterTransactionId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_StateTransactions_StateTransactionId",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "EnrollmentTransactions");

            migrationBuilder.DropTable(
                name: "InvocationTransactions");

            migrationBuilder.DropTable(
                name: "MinerTransaction");

            migrationBuilder.DropTable(
                name: "PublishTransactions");

            migrationBuilder.DropTable(
                name: "RegisterTransactions");

            migrationBuilder.DropTable(
                name: "StateDescriptors");

            migrationBuilder.DropTable(
                name: "TransactionAttributes");

            migrationBuilder.DropTable(
                name: "TransactionWitnesses");

            migrationBuilder.DropTable(
                name: "StateTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_EnrollmentTransactionId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_InvocationTransactionId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_MinerTransactionId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_PublishTransactionId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_RegisterTransactionId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_StateTransactionId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "EnrollmentTransactionId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "InvocationTransactionId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "MinerTransactionId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "PublishTransactionId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "RegisterTransactionId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "StateTransactionId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Transactions");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Transactions",
                nullable: false,
                oldClrType: typeof(byte));
        }
    }
}
