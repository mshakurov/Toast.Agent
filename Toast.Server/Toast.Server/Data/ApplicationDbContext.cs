using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Toast.Server.Data.Models;

namespace Toast.Server.Data
{
  public class ApplicationDbContext( DbContextOptions<ApplicationDbContext> options ) : IdentityDbContext<ApplicationUser>( options )
  {
    public DbSet<AgentCommandFor> AgentCommandFor { get; set; }

    public DbSet<AgentClient> AgentClient { get; set; }

    protected override void OnModelCreating( ModelBuilder builder )
    {
      base.OnModelCreating( builder );

      builder.Entity<AgentCommandFor>().HasKey( nameof( Models.AgentCommandFor.Id ) );

      builder.Entity<AgentClient>().HasKey( nameof( Models.AgentClient.ClientId ) );
    }
  }
}
