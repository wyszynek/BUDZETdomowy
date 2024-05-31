using HomeBudget.Models.Enum;
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

        public Time HowOften { get; set; }

        public DateTime FirstPaymentDate { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [CheckDate(ErrorMessage = "You cannot enter the date in the past!")]
        public DateTime LastPaymentDate { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
