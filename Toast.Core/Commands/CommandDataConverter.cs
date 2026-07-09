namespace Toast.Core.Commands
{
  internal static class CommandDataConverter
  {
    public static (TTarget? data, string? error) GetData<TTarget>( AgentCommand command )
    {
      TTarget? data = default;

      try
      {
        data = command.Parameters.Deserialize<TTarget>();
      }
      catch ( Exception ex )
      {
        return (default, $"# Ошибка десериализации Parameters а '{typeof( TTarget ).Name}': {ex.Message}|{ex.InnerException?.Message}|{ex.InnerException?.InnerException?.Message}");
      }

      if ( data == null )
      {
        return (default, "Parameters десериализовался в (TTarget?) null");
      }

      return (data, null);
    }
  }
}
