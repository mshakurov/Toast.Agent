using Toast.Core.Utilities;

namespace Toast.Core.Commands
{
  internal static class CommandDataConverter
  {
    public static (TTarget? data, string? error) GetData<TTarget>( AgentCommand command )
    {
      TTarget? data = default;

      try
      {
        data = JsonSerializer.Deserialize<TTarget>( command.JsonParameters );
      }
      catch ( Exception ex )
      {
        return (default, $"# Ошибка десериализации Parameters а '{typeof( TTarget ).Name}': {ex.GetFullMessage()}");
      }

      if ( data == null )
      {
        return (default, "Parameters десериализовался в (TTarget?) null");
      }

      return (data, null);
    }
  }
}
