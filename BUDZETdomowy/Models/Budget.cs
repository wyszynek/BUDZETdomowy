using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BUDZETdomowy.Models
{
    public class CheckDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            DateTime d = Convert.ToDateTime(value);
            return d >= DateTime.Now;
        }
    }

    public class Budget
    {
        [Key]
        public int BudgetId { get; set; }

        [Required(ErrorMessage = "Please enter the budget name")]
        [StringLength(25, MinimumLength = 2, ErrorMessage = "Name should be between 2 and 25 characters")]
        public string BudgetName { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select an account.")]
        public int AccountId { get; set; }
        public Account? Account { get; set; }

        [Required(ErrorMessage = "Please enter the limit")]
        [Column(TypeName = "decimal(18, 2)")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal Limit { get; set; }

        public DateTime CreationTime { get; set; } = DateTime.Now;

        [Required]
        [DataType(DataType.DateTime)]
        [CheckDate(ErrorMessage = "You cannot enter the date in the past!")]
        public DateTime EndTime { get; set; } = DateTime.Now;
    }
}
