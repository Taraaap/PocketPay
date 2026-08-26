namespace PocketPay.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(
        Guid userId,
        string email,
        string fullName);

    string GenerateRefreshToken();
}