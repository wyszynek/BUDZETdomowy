using Microsoft.AspNetCore.Mvc.DataAnnotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeBudget.Models
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
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter the budget name")]
        [StringLength(25, MinimumLength = 2, ErrorMessage = "Name should be between 2 and 25 characters")]
        public string BudgetName { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select an account.")]
        public int AccountId { get; set; }
        public Account? Account { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Limit should be greater than 0.")]
        [Column(TypeName = "decimal(18, 2)")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal Limit { get; set; }

        //chyba potrzebna jeszcze zmienna ktora bedzie przetrzymywac sume wszystkich transakcji danej kategorii, 
        //dzieki ktorej bedzie mozna latwiej obliczyc postep zakonczenia budzetu

        public DateTime CreationTime { get; set; } = DateTime.Now;

        [Required]
        [DataType(DataType.DateTime)]
        [CheckDate(ErrorMessage = "You cannot enter the date in the past!")]
        public DateTime EndTime { get; set; } = DateTime.Now;

        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
