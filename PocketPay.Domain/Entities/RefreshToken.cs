namespace PocketPay.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? RevokedAt { get; set; }

    public bool IsRevoked => RevokedAt.HasValue;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}