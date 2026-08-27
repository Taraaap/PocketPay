using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PocketPay.Application.DTOs;
using PocketPay.Domain.Entities;
using PocketPay.Infrastructure.Data;
using System.Security.Claims;
using System.Transactions;


namespace PocketPay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly PocketPayDbContext _dbContext;

    public WalletController(PocketPayDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetWallet()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized();
        }

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var wallet = await _dbContext.Wallets
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.UserId == userId);

        if (wallet == null)
        {
            return NotFound(new
            {
                message = "Wallet not found."
            });
        }

        return Ok(new
        {
            wallet.Id,
            wallet.WalletNumber,
            wallet.Balance,
            wallet.IsActive,
            wallet.CreatedAt
        });
    }


    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit(DepositRequest request)
    {
        if (request.Amount <= 0)
        {
            return BadRequest(new
            {
                message = "Deposit amount must be greater than zero."
            });
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userIdClaim) ||
            !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var wallet = await _dbContext.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userId);

        if (wallet == null)
        {
            return NotFound(new
            {
                message = "Wallet not found."
            });
        }

        wallet.Balance += request.Amount;

        var transaction = new WalletTransaction
        {
            Id = Guid.NewGuid(),
            WalletId = wallet.Id,
            Amount = request.Amount,
            Type = "Deposit",
            Status = "Completed",
            Reference = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.WalletTransactions.Add(transaction);

        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            message = "Money deposited successfully.",
            amount = request.Amount,
            newBalance = wallet.Balance,
            transactionId = transaction.Id
        });
    }
}