using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;

namespace AuthService.Infrastructure.Data;

public partial class UserDbContext : DbContext
{
    public UserDbContext()
    {
    }

    public UserDbContext(DbContextOptions<UserDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=UserDB;Username=postgres;Password=postgres");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var userRoleConverter = new ValueConverter<UserRole?, string>(
            v => v == null ? "user" : v.ToString()!.ToLower(),
            v => string.IsNullOrEmpty(v) ? UserRole.User : Enum.Parse<UserRole>(v, true)
        );

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Users_pkey");

            entity.HasIndex(e => e.Email, "IX_Users_Email");
            entity.HasIndex(e => e.Role, "IX_Users_Role");
            entity.HasIndex(e => e.Email, "Users_Email_key").IsUnique();

            // ===== Propriétés de BaseEntity =====
            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()");
            
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("now()");
            
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(3);

            // ===== Propriétés d'identification =====
            entity.Property(e => e.UserName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(512);

            // ===== Propriétés de profil =====
            entity.Property(e => e.FirstName)
                .HasMaxLength(100);

            entity.Property(e => e.LastName)
                .HasMaxLength(100);

            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20);

            // ===== Propriété Role avec convertisseur =====
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasConversion(userRoleConverter)
                .HasDefaultValueSql("'user'::character varying");

            // ===== Propriétés de sécurité et statut =====
            entity.Property(e => e.IsEmailConfirmed)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(e => e.IsLocked)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(e => e.LockoutEnd)
                .HasColumnType("timestamp with time zone");

            // ===== Tokens de rafraîchissement =====
            entity.Property(e => e.RefreshToken)
                .HasMaxLength(500);

            entity.Property(e => e.RefreshTokenExpiryTime)
                .HasColumnType("timestamp with time zone");

            // ===== Tokens de réinitialisation =====
            entity.Property(e => e.ResetToken)
                .HasMaxLength(500);

            entity.Property(e => e.ResetTokenExpiryTime)
                .HasColumnType("timestamp with time zone");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

// ✅ Factory pour les migrations (IDesignTimeDbContextFactory)
public class UserDbContextFactory : IDesignTimeDbContextFactory<UserDbContext>
{
    public UserDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // ajuste si nécessaire
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var conn = config.GetConnectionString("AuthConnection");
        var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
        optionsBuilder.UseNpgsql(conn, b => b.MigrationsAssembly(typeof(UserDbContext).Assembly.FullName));
        return new UserDbContext(optionsBuilder.Options);
    }
}
