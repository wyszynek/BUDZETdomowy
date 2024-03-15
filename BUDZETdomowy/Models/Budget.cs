using System.ComponentModel.DataAnnotations;

namespace BUDZETdomowy.Models
{
    public class Budget
    {
        [Key]
        public int BudgetId { get; set; }

        public string BudgetName { get; set;}

        public int CategoryId { get; set; }

        public decimal Amount { get; set; }

        public bool IsShared { get; set; }
        public bool IsFinished { get; set; }

        public DateTime CreationTime { get; set; } = DateTime.Now;
        public DateTime EndTime { get; set; }
    }
}
