using CoupleChat.Models;
using Microsoft.EntityFrameworkCore;

namespace CoupleChat.Data;

public class ChatDbContext : DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }

    public DbSet<Message> Messages { get; set; } = null!;
    public DbSet<Response> Responses { get; set; } = null!;
    public DbSet<Expense> Expenses { get; set; } = null!;
    public DbSet<TravailDomestique> TravailDomestique { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Sender).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<Response>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.QuestionId).IsRequired();
            entity.Property(e => e.QuestionText).IsRequired();
            entity.Property(e => e.UserResponse).IsRequired();
            entity.Property(e => e.Category).IsRequired();
            entity.Property(e => e.Person).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Label).IsRequired();
            entity.Property(e => e.Amount).IsRequired();
            entity.Property(e => e.PaidBy).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<TravailDomestique>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Sexe).IsRequired();
            entity.Property(e => e.Activite).IsRequired();
            entity.Property(e => e.TrancheAge).IsRequired();
            entity.Property(e => e.DureeMinutes).IsRequired();
            entity.Property(e => e.DureeHeures).IsRequired();
            entity.Property(e => e.CoutJour).IsRequired();
            entity.HasIndex(e => new { e.Sexe, e.Activite, e.TrancheAge }).IsUnique();
        });

        // Seed données INSEE (valeur horaire estimée à 15€/heure)
        // Note: Seeding is done in Program.cs after EnsureCreated()
    }
}
