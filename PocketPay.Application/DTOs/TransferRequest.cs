namespace PocketPay.Application.DTOs;

public class TransferRequest
{
    public string ReceiverWalletNumber { get; set; } = string.Empty;

    public decimal Amount { get; set; }
}