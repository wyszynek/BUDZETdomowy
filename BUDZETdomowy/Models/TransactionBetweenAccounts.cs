﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HomeBudget.Models
{
    public class TransactionBetweenAccounts
    {
        [Key]
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a sender account.")]
        public int? SenderId { get; set; }
        public Account? SenderAccount { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a recipient account.")]
        public int? RecipientId { get; set; }
        public Account? RecipientAccount { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount should be greater than 0.")]
        [Column(TypeName = "decimal(18, 2)")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public decimal Amount { get; set; }

        [Column(TypeName = "nvarchar(75)")]
        public string? Note { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        [Range(1, int.MaxValue, ErrorMessage = "Please select the currency.")]
        public int CurrencyId { get; set; }
        public Currency? Currency { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
