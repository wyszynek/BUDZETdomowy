using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HomeBudget.Models;

namespace HomeBudget.Models
{
    public class TransactionViewModel
    {
        [Required(ErrorMessage = "Source of income is required.")]
        public int? SourceOfIncomeId { get; set; }
        public SourceOfIncome? SourceOfIncome { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        public DateTime Date { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select an account.")]
        public int AccountId { get; set; }
        public Account? Account { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select the currency.")]
        public int CurrencyId { get; set; }
        public Currency? Currency { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
