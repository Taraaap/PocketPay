using Microsoft.AspNetCore.Identity;
using PocketPay.Application.DTOs;
using PocketPay.Application.Interfaces;
using PocketPay.Domain.Entities;
using PocketPay.Infrastructure.Data;
using PocketPay.Infrastructure.Identity;

namespace PocketPay.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtService _jwtService;
    private readonly PocketPayDbContext _dbContext;

    private string GenerateWalletNumber()
    {
        return "PP" + Random.Shared.Next(10000000, 99999999);
    }
    public AuthService(
        UserManager<ApplicationUser> userManager,
        IJwtService jwtService,
        PocketPayDbContext dbContext)
    {
        _userManager = userManager;
        _jwtService = jwtService;
         _dbContext = dbContext;

    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);

        if (existingUser != null)
        {
            throw new Exception("Email is already registered.");
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(
            user,
            request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(
                ", ",
                result.Errors.Select(e => e.Description));

            throw new Exception(errors);
        }

        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            WalletNumber = GenerateWalletNumber(),
            Balance = 0.00m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Wallets.Add(wallet);

        await _dbContext.SaveChangesAsync();

        return new AuthResponse
        {
            UserId = user.Id.ToString(),
            FullName = user.FullName,
            Email = user.Email!,
            Message = "Registration successful."
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            throw new Exception("Invalid email or password.");
        }

        var passwordValid = await _userManager.CheckPasswordAsync(
            user,
            request.Password);

        if (!passwordValid)
        {
            throw new Exception("Invalid email or password.");
        }

        var accessToken = _jwtService.GenerateAccessToken(
            user.Id,
            user.Email!,
            user.FullName);

        var refreshToken = _jwtService.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.RefreshTokens.Add(refreshTokenEntity);

        await _dbContext.SaveChangesAsync();

        return new AuthResponse
        {
            UserId = user.Id.ToString(),
            FullName = user.FullName,
            Email = user.Email!,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15),
            Message = "Login successful."
        };
    }
}