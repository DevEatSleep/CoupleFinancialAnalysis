using CoupleChat.Models;
using Microsoft.EntityFrameworkCore;

namespace CoupleChat.Data;

public class ChatDbContext : DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Couple> Couples { get; set; } = null!;
    public DbSet<Message> Messages { get; set; } = null!;
    public DbSet<Response> Responses { get; set; } = null!;
    public DbSet<Expense> Expenses { get; set; } = null!;
    public DbSet<TravailDomestique> TravailDomestique { get; set; } = null!;
    public DbSet<DomestiqueResponse> DomestiqueResponses { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.CoupleId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasOne(e => e.Couple)
                .WithMany(c => c.Users)
                .HasForeignKey(e => e.CoupleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Couple entity
        modelBuilder.Entity<Couple>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CreatedAt).IsRequired();
        });
        
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Sender).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.CoupleId).IsRequired();
            entity.HasOne(e => e.Couple)
                .WithMany(c => c.Messages)
                .HasForeignKey(e => e.CoupleId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.CoupleId);
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
            entity.Property(e => e.CoupleId).IsRequired();
            entity.HasOne(e => e.Couple)
                .WithMany(c => c.Responses)
                .HasForeignKey(e => e.CoupleId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.CoupleId);
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Label).IsRequired();
            entity.Property(e => e.Amount).IsRequired();
            entity.Property(e => e.PaidBy).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.CoupleId).IsRequired();
            entity.HasOne(e => e.Couple)
                .WithMany(c => c.Expenses)
                .HasForeignKey(e => e.CoupleId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.CoupleId);
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
            // CoupleId is optional - NULL for reference data, set for couple-specific data
            entity.HasOne(e => e.Couple)
                .WithMany(c => c.TravailDomestiques)
                .HasForeignKey(e => e.CoupleId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.Sexe, e.Activite, e.TrancheAge }).IsUnique();
            entity.HasIndex(e => e.CoupleId);
        });

        modelBuilder.Entity<DomestiqueResponse>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Person).IsRequired();
            entity.Property(e => e.Activite).IsRequired();
            entity.Property(e => e.HeuresParSemaine).IsRequired();
            entity.Property(e => e.InseeRefFemme).IsRequired();
            entity.Property(e => e.InseeRefHomme).IsRequired();
            entity.Property(e => e.ValeurMonetaire).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.CoupleId).IsRequired();
            entity.HasOne(e => e.Couple)
                .WithMany(c => c.DomestiqueResponses)
                .HasForeignKey(e => e.CoupleId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.Person, e.Activite, e.CoupleId }).IsUnique();
            entity.HasIndex(e => e.CoupleId);
        });

        // Seed données INSEE (valeur horaire estimée à 15€/heure)
        // Note: Seeding is done in Program.cs after EnsureCreated()
    }
}
