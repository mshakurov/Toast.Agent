using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Toast.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddAgentCommandForTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AgentCommand",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JsonParameters = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentCommand", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AgentCommandFor",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CommandId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentCommandFor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentCommandFor_AgentCommand_CommandId",
                        column: x => x.CommandId,
                        principalTable: "AgentCommand",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommandFor_CommandId",
                table: "AgentCommandFor",
                column: "CommandId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentCommandFor");

            migrationBuilder.DropTable(
                name: "AgentCommand");
        }
    }
}
