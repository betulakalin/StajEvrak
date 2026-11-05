using Microsoft.EntityFrameworkCore;
using StajEvrak.Core.Entities;

namespace StajEvrak.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<StudentRecord> StudentRecords => Set<StudentRecord>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<LoginLog> LoginLogs => Set<LoginLog>();
    public DbSet<Ek16Data> Ek16Data => Set<Ek16Data>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Kalıtım veritabanında da görünür: tek tablo + UserType sütunu
        b.Entity<User>()
            .HasDiscriminator<string>("UserType")
            .HasValue<Student>("Student")
            .HasValue<Admin>("Admin");

        b.Entity<User>().HasIndex(u => u.Username).IsUnique();
        b.Entity<User>().HasIndex(u => u.Email).IsUnique();
        b.Entity<StudentRecord>().HasIndex(r => r.StudentNo).IsUnique();

        b.Entity<Document>().Property(d => d.Status).HasConversion<string>();
        // Her belge için en fazla 1 manuel + 1 okunan kayıt olabilir
        b.Entity<Ek16Data>().HasIndex(x => new { x.DocumentId, x.Source }).IsUnique();
        b.Entity<Ek16Data>().Property(x => x.Source).HasConversion<string>();
    }
}