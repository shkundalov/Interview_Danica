using InterviewDanica.Api.Models;
using Microsoft.EntityFrameworkCore;
namespace InterviewDanica.Api.Data;

public class DBContext : DbContext {
    public DBContext(DbContextOptions<DBContext> options) : base(options) { }
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Template> Templates { get; set; } = null!;
    public DbSet<ProcessedRequest> ProcessedRequests { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Customer>(entity => {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique(); //Used for idempotency
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.ToTable("Customers");
        });
        modelBuilder.Entity<Template>(entity => {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique(); //Unique for idempotency
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Body).IsRequired().HasMaxLength(5000);
            entity.ToTable("Templates");
        });
        modelBuilder.Entity<ProcessedRequest>(entity => {
            entity.HasKey(e => e.IdempotencyKey);
            entity.Property(e => e.StatusCode).IsRequired();
            entity.Property(e => e.RequestData).IsRequired();
            entity.Property(e => e.ResponseData).IsRequired();
            entity.Property(e => e.ProcessedAt).IsRequired();
            entity.ToTable("ProcessedRequests");
        });
    }
}
