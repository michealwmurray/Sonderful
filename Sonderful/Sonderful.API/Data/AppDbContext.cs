using Microsoft.EntityFrameworkCore;
using Sonderful.API.Models;

namespace Sonderful.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Plan> Plans { get; set; }
    public DbSet<Rsvp> Rsvps { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<SonderScore> SonderScores { get; set; }
    public DbSet<PlanPhoto> PlanPhotos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Enforce unique email and username at the database level
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<Plan>()
            .HasOne(p => p.Creator)
            .WithMany(u => u.CreatedPlans)
            .HasForeignKey(p => p.CreatorId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Rsvp>()
            .HasOne(r => r.Plan)
            .WithMany(p => p.Rsvps)
            .HasForeignKey(r => r.PlanId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Rsvp>()
            .HasOne(r => r.User)
            .WithMany(u => u.Rsvps)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Prevent a user from RSVPing to the same plan twice
        modelBuilder.Entity<Rsvp>()
            .HasIndex(r => new { r.PlanId, r.UserId })
            .IsUnique();

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Plan)
            .WithMany(p => p.Comments)
            .HasForeignKey(c => c.PlanId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SonderScore>()
            .HasOne(s => s.Rater)
            .WithMany(u => u.SonderScoresGiven)
            .HasForeignKey(s => s.RaterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SonderScore>()
            .HasOne(s => s.RatedUser)
            .WithMany(u => u.SonderScoresReceived)
            .HasForeignKey(s => s.RatedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SonderScore>()
            .HasOne(s => s.Plan)
            .WithMany(p => p.SonderScores)
            .HasForeignKey(s => s.PlanId)
            .OnDelete(DeleteBehavior.Cascade);

        // One score per (plan, rater, rated-user) combination
        modelBuilder.Entity<SonderScore>()
            .HasIndex(s => new { s.PlanId, s.RaterId, s.RatedUserId })
            .IsUnique();

        modelBuilder.Entity<PlanPhoto>()
            .HasOne(p => p.Plan)
            .WithMany(p => p.Photos)
            .HasForeignKey(p => p.PlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
