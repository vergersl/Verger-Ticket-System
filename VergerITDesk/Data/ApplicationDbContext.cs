using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VergerITDesk.Models;

namespace VergerITDesk.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();
    public DbSet<KpiDefinition> KpiDefinitions => Set<KpiDefinition>();
    public DbSet<KpiActual> KpiActuals => Set<KpiActual>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Ticket>(t =>
        {
            t.Property(x => x.Priority).HasConversion<string>();
            t.Property(x => x.Status).HasConversion<string>();

            t.HasOne(x => x.Requester)
                .WithMany()
                .HasForeignKey(x => x.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);

            t.HasMany(x => x.Comments)
                .WithOne(c => c.Ticket)
                .HasForeignKey(c => c.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            t.HasIndex(x => x.Seq).IsUnique();
        });

        builder.Entity<KpiActual>()
            .HasIndex(x => new { x.KpiRef, x.Month })
            .IsUnique();
    }
}
