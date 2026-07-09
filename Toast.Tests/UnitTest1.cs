using System.Text.Json;

using Toast.Core.Commands;
using Toast.Core.Commands.CommandData;

namespace Toast.Tests
{
  public class UnitTest1
  {
    [Fact]
    public void Test1()
    {
      var sm = new ShowMessageData { Message = "Hallow device!", Title = $"From server! You are: {111}", Duration = 11, WaitIfShow = false };
      AgentResponse resp = new() { Commands = [ new AgentCommand { Id = Guid.NewGuid(), Type = CommandTypes.ShowMessage, JsonParameters = JsonSerializer.Serialize( sm ) } ] };

      Assert.NotNull( resp );

      var respStr = JsonSerializer.Serialize(resp );

      Assert.NotNull( respStr );

      var resp2 = JsonSerializer.Deserialize<AgentResponse>( respStr );

      Assert.NotNull( resp2 );

      Assert.Equal( resp.Commands.Count, resp2.Commands.Count );
      Assert.Equal( resp.Commands[0].Type, resp2.Commands[0].Type );
      Assert.Equal( resp.Commands[0].Id, resp2.Commands[0].Id );
      Assert.Equal( resp.Commands[0].JsonParameters, resp2.Commands[0].JsonParameters );

      var sm2 = JsonSerializer.Deserialize<ShowMessageData>( resp2.Commands[0].JsonParameters );
      Assert.NotNull( sm2 );

      Assert.Equal( sm.Title, sm2.Title );
      Assert.Equal( sm.Message, sm2.Message );
      Assert.Equal( sm.Duration, sm2.Duration );
      Assert.Equal( sm.WaitIfShow, sm2.WaitIfShow );
    }
  }
}
