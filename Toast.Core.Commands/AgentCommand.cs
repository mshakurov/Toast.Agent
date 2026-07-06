using System.Text.Json;

namespace Toast.Core.Commands;

public sealed class AgentCommand
{
  public Guid Id { get; set; }

  public string Type { get; set; } = string.Empty;

  public JsonElement Parameters { get; set; }
}