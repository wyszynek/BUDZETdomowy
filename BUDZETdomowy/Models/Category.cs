using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BUDZETdomowy.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Please enter the name of the category")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Category should be between 2 and 20 characters")]
        public string CategoryName { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string Icon { get; set; } = "";

        [Column(TypeName = "nvarchar(50)")]
        public string Type { get; set; } = "Expense";
    }
}
