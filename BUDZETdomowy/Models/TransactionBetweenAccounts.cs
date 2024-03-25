﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BUDZETdomowy.Models
{
    public class TransactionBetweenAccounts
    {
        [Key]
        public int TransactionId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a sender account.")]
        public int? SenderId { get; set; }
        public Account? SenderAccount { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a recipient account.")]
        public int? RecipientId { get; set; }
        public Account? RecipientAccount { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Amount should be greater than 0.")]
        [Column(TypeName = "decimal(18, 2)")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal Amount { get; set; }

        [Column(TypeName = "nvarchar(75)")]
        public string? Note { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;
    }
}
