using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using HomeBudget.Data;
using HomeBudget.Models;

public class RecurringPaymentService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RecurringPaymentService> _logger;

    public RecurringPaymentService(IServiceProvider serviceProvider, ILogger<RecurringPaymentService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Recurring Payment Service running at: {time}", DateTimeOffset.Now);
            await ProcessPaymentsAsync();
            await Task.Delay(5000, stoppingToken); // Wait for 5 seconds
        }
    }

    private async Task ProcessPaymentsAsync()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var currentDate = DateTime.UtcNow;
            var payments = context.ReccuringPayments
                .Where(p => p.LastSuccessfulPayment == null || p.LastSuccessfulPayment <= currentDate)
                .ToList();

            foreach (var payment in payments)
            {
                var account = context.Accounts.Find(payment.AccountId);
                if (account != null)
                {
                    var transaction = new Transaction
                    {
                        CategoryId = payment.CategoryId,
                        AccountId = payment.AccountId,
                        Amount = payment.Amount,
                        Note = "Recurring payment",
                        Date = DateTime.Now,
                        CurrencyId = payment.CurrencyId,
                        UserId = payment.UserId
                    };

                    context.Transactions.Add(transaction);

                    account.Income -= payment.Amount;
                    account.Expanse += payment.Amount;
                    payment.LastSuccessfulPayment = currentDate;
                    context.Update(account);
                    context.Update(payment);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
