using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PocketPay.Domain.Entities;
using PocketPay.Infrastructure.Identity;

namespace PocketPay.Infrastructure.Data;

public class PocketPayDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public PocketPayDbContext(
        DbContextOptions<PocketPayDbContext> options)
        : base(options)
    {
    }

    public DbSet<Wallet> Wallets => Set<Wallet>();

    public DbSet<WalletTransaction> WalletTransactions =>Set<WalletTransaction>();

    public DbSet<RefreshToken> RefreshTokens =>Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Wallet>(entity =>
        {
            entity.HasKey(w => w.Id);

            entity.Property(w => w.WalletNumber)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(w => w.Balance)
                .HasPrecision(18, 2);

            entity.Property(w => w.IsActive)
                .IsRequired();

            entity.Property(w => w.CreatedAt)
                .IsRequired();

            entity.HasIndex(w => w.WalletNumber)
                .IsUnique();
            builder.Entity<Wallet>().Property(w => w.RowVersion)
                .IsRowVersion();
        });

        builder.Entity<WalletTransaction>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Amount)
                .HasPrecision(18, 2);

            entity.Property(t => t.Type)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(t => t.Status)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(t => t.Reference)
                .HasMaxLength(100);

            entity.Property(t => t.CreatedAt)
                .IsRequired();

            entity.HasOne<Wallet>()
                .WithMany()
                .HasForeignKey(t => t.WalletId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Token)
                .IsRequired()
                .HasMaxLength(500);

            entity.HasIndex(x => x.Token)
                .IsUnique();

            entity.Property(x => x.ExpiresAt)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .IsRequired();
        });
    }
}