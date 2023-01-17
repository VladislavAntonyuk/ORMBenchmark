using System.Configuration;
using Microsoft.EntityFrameworkCore;
using OrmBenchmark.Database;

namespace OrmBenchmark.EfCore;

public sealed class OlympiadContext : DbContext
{
    public DbSet<Player> Players { get; set; }
    public DbSet<Sport> Sports { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<Olympiad> Olympics { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Olympiad>()
            .HasMany(e => e.Sports);

        modelBuilder.Entity<Sport>()
            .HasOne(x => x.Olympiad).WithMany(x => x.Sports);

        modelBuilder.Entity<Team>()
            .HasOne(x => x.Sport).WithMany(x => x.Teams);

        modelBuilder.Entity<Player>()
            .HasOne(x => x.Team).WithMany(x => x.Players);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString);
        base.OnConfiguring(optionsBuilder);
    }
}