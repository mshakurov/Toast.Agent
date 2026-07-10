using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Toast.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddAgentResultsDBTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AgentResultDB",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgentId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentResultDB", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommandResult",
                columns: table => new
                {
                    CommandId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AgentResultDBId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommandResult", x => x.CommandId);
                    table.ForeignKey(
                        name: "FK_CommandResult_AgentResultDB_AgentResultDBId",
                        column: x => x.AgentResultDBId,
                        principalTable: "AgentResultDB",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommandResult_AgentResultDBId",
                table: "CommandResult",
                column: "AgentResultDBId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommandResult");

            migrationBuilder.DropTable(
                name: "AgentResultDB");
        }
    }
}
