using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Toast.Server.Migrations
{
  /// <inheritdoc />
  public partial class AddLastSessIdToClient : Migration
  {
    /// <inheritdoc />
    protected override void Up( MigrationBuilder migrationBuilder )
    {
      migrationBuilder.AddColumn<long>(
          name: "LastAgentSessionId",
          table: "AgentClient",
          type: "bigint",
          nullable: false,
          defaultValue: 0L );
    }

    /// <inheritdoc />
    protected override void Down( MigrationBuilder migrationBuilder )
    {
      migrationBuilder.DropColumn(
          name: "LastAgentSessionId",
          table: "AgentClient" );
    }
  }
}
