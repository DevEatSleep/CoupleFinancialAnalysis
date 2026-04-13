using CoupleChat.Models;
using Microsoft.EntityFrameworkCore;

namespace CoupleChat.Data;

public class ChatDbContext : DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }

    public DbSet<Message> Messages { get; set; } = null!;
    public DbSet<Response> Responses { get; set; } = null!;
    public DbSet<Expense> Expenses { get; set; } = null!;

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
    }
}
