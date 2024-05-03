using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeBudget.Models
{
    public class Account
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter name of the account")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name should be between 2 and 20 characters")]
        public string AccountName { get; set; }

        [Column(TypeName = "nvarchar(75)")]
        public string? Note { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public decimal Income { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public decimal Expanse { get; set; }

        public decimal Balance => Income - Expanse;

        public Account()
        {
            Expanse = 0;
        }

        [Range(1, int.MaxValue, ErrorMessage = "Please select the currency.")]
        public int CurrencyId { get; set; }
        public Currency? Currency { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
