using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HomeBudget.Models;

namespace HomeBudget.Models
{
    public class TransactionViewModel
    {
        [Required(ErrorMessage = "Source of income is required.")]
        public int? SourceOfIncomeId { get; set; }
        public SourceOfIncome? SourceOfIncome { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount should be greater than 0.")]
        [Column(TypeName = "decimal(18, 2)")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
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
