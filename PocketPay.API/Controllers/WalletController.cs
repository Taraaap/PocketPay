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

        if (decimal.Round(request.Amount, 2) != request.Amount)
        {
            return BadRequest(new
            {
                message = "Amount can have a maximum of 2 decimal places."
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

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userIdClaim) ||
            !Guid.TryParse(userIdClaim, out var userId))
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

        var transactions = await _dbContext.WalletTransactions
            .AsNoTracking()
            .Where(t => t.WalletId == wallet.Id)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new
            {
                t.Id,
                t.Amount,
                t.Type,
                t.Status,
                t.Reference,
                t.CreatedAt
            })
            .ToListAsync();

        return Ok(transactions);
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer(TransferRequest request)
    {
        if (request.Amount <= 0)
        {
            return BadRequest(new
            {
                message = "Transfer amount must be greater than zero."
            });
        }

        if (decimal.Round(request.Amount, 2) != request.Amount)
        {
            return BadRequest(new
            {
                message = "Amount can have a maximum of 2 decimal places."
            });
        }

        if (string.IsNullOrWhiteSpace(request.ReceiverWalletNumber))
        {
            return BadRequest(new
            {
                message = "Receiver wallet number is required."
            });
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userIdClaim) ||
            !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var senderWallet = await _dbContext.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userId);

        if (senderWallet == null)
        {
            return NotFound(new
            {
                message = "Sender wallet not found."
            });
        }

        var receiverWallet = await _dbContext.Wallets
            .FirstOrDefaultAsync(w =>
                w.WalletNumber == request.ReceiverWalletNumber);

        if (receiverWallet == null)
        {
            return NotFound(new
            {
                message = "Receiver wallet not found."
            });
        }

        if (senderWallet.Id == receiverWallet.Id)
        {
            return BadRequest(new
            {
                message = "You cannot transfer money to your own wallet."
            });
        }

        if (!senderWallet.IsActive || !receiverWallet.IsActive)
        {
            return BadRequest(new
            {
                message = "One of the wallets is inactive."
            });
        }

        if (senderWallet.Balance < request.Amount)
        {
            return BadRequest(new
            {
                message = "Insufficient balance."
            });
        }

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync();

        try
        {
            senderWallet.Balance -= request.Amount;
            receiverWallet.Balance += request.Amount;

            var reference = Guid.NewGuid().ToString();

            var senderTransaction = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = senderWallet.Id,
                Amount = request.Amount,
                Type = "Transfer",
                Status = "Completed",
                Reference = reference,
                CreatedAt = DateTime.UtcNow
            };

            var receiverTransaction = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = receiverWallet.Id,
                Amount = request.Amount,
                Type = "Transfer",
                Status = "Completed",
                Reference = reference,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.WalletTransactions.Add(senderTransaction);
            _dbContext.WalletTransactions.Add(receiverTransaction);

            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            return Ok(new
            {
                message = "Transfer successful.",
                amount = request.Amount,
                reference,
                newBalance = senderWallet.Balance
            });
        }
      
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();

            return Conflict(new
            {
                message = "The wallet was updated by another transaction. Please try again."
            });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}