using System.Text.Json;

namespace Toast.Core.Commands;

public sealed class AgentCommand
{
  public static readonly AgentCommand Empty = new ();

  public Guid Id { get; set; } = Guid.Empty;

  public string Type { get; set; } = string.Empty;

  public string JsonParameters { get; set; } = string.Empty;
}