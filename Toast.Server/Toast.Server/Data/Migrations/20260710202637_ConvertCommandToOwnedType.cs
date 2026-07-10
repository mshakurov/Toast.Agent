using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Toast.Server.Migrations
{
    /// <inheritdoc />
    public partial class ConvertCommandToOwnedType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AgentCommandFor_AgentCommand_CommandId",
                table: "AgentCommandFor");

            migrationBuilder.DropTable(
                name: "AgentCommand");

            migrationBuilder.DropIndex(
                name: "IX_AgentCommandFor_CommandId",
                table: "AgentCommandFor");

            migrationBuilder.RenameColumn(
                name: "CommandId",
                table: "AgentCommandFor",
                newName: "Command_Id");

            migrationBuilder.AddColumn<string>(
                name: "Command_JsonParameters",
                table: "AgentCommandFor",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Command_Type",
                table: "AgentCommandFor",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Command_JsonParameters",
                table: "AgentCommandFor");

            migrationBuilder.DropColumn(
                name: "Command_Type",
                table: "AgentCommandFor");

            migrationBuilder.RenameColumn(
                name: "Command_Id",
                table: "AgentCommandFor",
                newName: "CommandId");

            migrationBuilder.CreateTable(
                name: "AgentCommand",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JsonParameters = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentCommand", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommandFor_CommandId",
                table: "AgentCommandFor",
                column: "CommandId");

            migrationBuilder.AddForeignKey(
                name: "FK_AgentCommandFor_AgentCommand_CommandId",
                table: "AgentCommandFor",
                column: "CommandId",
                principalTable: "AgentCommand",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
