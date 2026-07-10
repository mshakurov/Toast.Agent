using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Toast.Core.Commands;
using Toast.Server.Data.Models;

namespace Toast.Server.Data
{
  public class ApplicationDbContext( DbContextOptions<ApplicationDbContext> options ) : IdentityDbContext<ApplicationUser>( options )
  {
    public DbSet<AgentCommandFor> AgentCommandFor { get; set; }

    public DbSet<AgentClient> AgentClient { get; set; }

    public DbSet<AgentResultDB> AgentResultDB { get; set; }

    protected override void OnModelCreating( ModelBuilder builder )
    {
      base.OnModelCreating( builder );

      builder.Entity<AgentCommandFor>().HasKey( c => c.Id );

      // 2. ГАРАНТИРУЕМ АВТОИНКРЕМЕНТ (IDENTITY) ДЛЯ SQL SERVER
      builder.Entity<AgentCommandFor>()
          .Property( c => c.Id )
          .UseIdentityColumn(); // Создаст в базе IDENTITY(1,1)

      builder.Entity<AgentClient>().HasKey( c => c.ClientId );

      // ---- НАСТРОЙКА СВЯЗИ ----
      builder.Entity<AgentCommandFor>()
          .HasOne( c => c.Client )                  // У команды есть один клиент
          .WithMany()                             // У клиента может быть много команд (коллекцию в AgentClient мы не создавали)
          .HasForeignKey( c => c.ClientId )    // Внешний ключ в таблице команд
          .IsRequired( true )                      // Делает связь необязательной (так как тип string? допускает null)
          .OnDelete( DeleteBehavior.ClientNoAction );      // Что делать при удалении клиента (Cascade - удалит и его команды)

      // Говорим EF Core, что AgentCommand — это неотъемлемая часть AgentCommandFor
      builder.Entity<AgentCommandFor>()
             .OwnsOne( c => c.Command );

      builder.Entity<AgentResultDB>().HasKey( c => c.Id );

      builder.Entity<CommandResult>()
        .HasKey( c => c.CommandId );
    }
  }
}
