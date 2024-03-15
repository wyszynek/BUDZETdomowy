using System.ComponentModel.DataAnnotations;

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
        public string BudgetName { get; set;}

        [Required]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Please enter the amount")]
        public decimal Amount { get; set; }

        [Required]
        public bool IsShared { get; set; }

        [Required]
        public bool IsFinished { get; set; }

        public DateTime CreationTime { get; set; } = DateTime.Now;

        [Required]
        [DataType(DataType.DateTime)]
        [CheckDate(ErrorMessage = "You cannot enter the date in the past!")]
        public DateTime EndTime { get; set; }
    }
}
