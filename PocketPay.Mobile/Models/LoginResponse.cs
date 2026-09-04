namespace PocketPay.Mobile.Models;

public class LoginResponse
{
    public string UserId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? AccessToken { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? AccessTokenExpiresAt { get; set; }

    public string Message { get; set; } = string.Empty;
}