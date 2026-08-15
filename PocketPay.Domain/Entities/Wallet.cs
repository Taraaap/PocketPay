namespace PocketPay.Domain.Entities;

public class Wallet
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string WalletNumber { get; set; } = string.Empty;

    public decimal Balance { get; set; } = 0.00m;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}