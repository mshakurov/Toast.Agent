using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Toast.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeFieldsToAgentCommansForTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "AgentCommandFor",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "Sent",
                table: "AgentCommandFor",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Created",
                table: "AgentCommandFor");

            migrationBuilder.DropColumn(
                name: "Sent",
                table: "AgentCommandFor");
        }
    }
}
