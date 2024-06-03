using DocumentFormat.OpenXml.Drawing.Diagrams;
using HomeBudget.Data;
using HomeBudget.Models.Enum;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeBudget.Models
{
    public class ReccuringPayment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter the name of the payment")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Category should be between 2 and 50 characters")]
        public string Title { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select an account.")]
        public int AccountId { get; set; }
        public Account? Account { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount should be greater than 0.")]
        [Column(TypeName = "decimal(18, 2)")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public decimal Amount { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select the currency.")]
        public int CurrencyId { get; set; }
        public Currency? Currency { get; set; }

        public ReccuringPaymentFrequency HowOften { get; set; }

        public DateTime FirstPaymentDate { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [CheckDate(ErrorMessage = "You cannot enter the date in the past!")]
        public DateTime LastPaymentDate { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? LastSuccessfulPayment { get; set; }
    }

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
}
