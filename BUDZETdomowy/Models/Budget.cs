using System.ComponentModel.DataAnnotations;

namespace BUDZETdomowy.Models
{
    public class Budget
    {
        [Key]
        public int BudgetId { get; set; } //

        public int Type { get; set; }

        public DateTime CreationTime { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        //[CheckDate(ErrorMessage = "You cannot enter the date in the past!")]
        public DateTime DesiredTime { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public double Amount { get; set; }

        [Required]
        public bool IsFinished { get; set; }
    }
}
