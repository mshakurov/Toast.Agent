using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Toast.Server.Migrations
{
    /// <inheritdoc />
    public partial class FixCommandResultCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommandResult_AgentResultDB_AgentResultDBId",
                table: "CommandResult");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CommandResult",
                table: "CommandResult");

            migrationBuilder.AlterColumn<long>(
                name: "AgentResultDBId",
                table: "CommandResult",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "CommandResult",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CommandResult",
                table: "CommandResult",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CommandResult_AgentResultDB_AgentResultDBId",
                table: "CommandResult",
                column: "AgentResultDBId",
                principalTable: "AgentResultDB",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommandResult_AgentResultDB_AgentResultDBId",
                table: "CommandResult");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CommandResult",
                table: "CommandResult");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "CommandResult");

            migrationBuilder.AlterColumn<long>(
                name: "AgentResultDBId",
                table: "CommandResult",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CommandResult",
                table: "CommandResult",
                column: "CommandId");

            migrationBuilder.AddForeignKey(
                name: "FK_CommandResult_AgentResultDB_AgentResultDBId",
                table: "CommandResult",
                column: "AgentResultDBId",
                principalTable: "AgentResultDB",
                principalColumn: "Id");
        }
    }
}
