using Microsoft.AspNetCore.Identity;
using PocketPay.Application.DTOs;
using PocketPay.Application.Interfaces;
using PocketPay.Infrastructure.Identity;

namespace PocketPay.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
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

        return new AuthResponse
        {
            UserId = user.Id.ToString(),
            FullName = user.FullName,
            Email = user.Email!,
            Message = "Login successful."
        };
    }
}