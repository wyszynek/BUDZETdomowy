using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BUDZETdomowy.Models
{
    public class Account
    {
        [Key]
        public int AccountId { get; set; }

        [Required(ErrorMessage = "Please enter name of the account")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name should be between 2 and 20 characters")]
        public string AccountName { get; set; }

        public string? Note { get; set; }

        [Required(ErrorMessage = "Please select a currency")]
        [ForeignKey("Currency")]
        public long CurrencyId { get; set; }

        public string CurrencySymbol { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal Income { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal Expanse { get; set; }

        public decimal Balance => Income - Expanse; 

        public string Display => AccountName + " (" + CurrencySymbol + ")";
    }
}
