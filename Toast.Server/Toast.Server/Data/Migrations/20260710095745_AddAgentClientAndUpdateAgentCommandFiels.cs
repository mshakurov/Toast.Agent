using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Toast.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddAgentClientAndUpdateAgentCommandFiels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ClientId",
                table: "AgentCommandFor",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "AgentClient",
                columns: table => new
                {
                    ClientId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentClient", x => x.ClientId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommandFor_ClientId",
                table: "AgentCommandFor",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_AgentCommandFor_AgentClient_ClientId",
                table: "AgentCommandFor",
                column: "ClientId",
                principalTable: "AgentClient",
                principalColumn: "ClientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AgentCommandFor_AgentClient_ClientId",
                table: "AgentCommandFor");

            migrationBuilder.DropTable(
                name: "AgentClient");

            migrationBuilder.DropIndex(
                name: "IX_AgentCommandFor_ClientId",
                table: "AgentCommandFor");

            migrationBuilder.AlterColumn<string>(
                name: "ClientId",
                table: "AgentCommandFor",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
