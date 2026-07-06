namespace Toast.Core.Commands;

public sealed class CommandResult
{
  public Guid CommandId { get; set; }

  public bool Success { get; set; }

  public string? Message { get; set; }
}