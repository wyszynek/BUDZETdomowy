using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BUDZETdomowy.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Required(ErrorMessage = "Please enter the title")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Title should be between 2 and 20 characters")]
        [Column(TypeName = "nvarchar(50)")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Please enter the type")]
        [Column(TypeName = "nvarchar(50)")]
        //(0 - wydatek, 1 - wplata, 2 - przelew)
        public string Type { get; set; }
        
        public DateTime Date { get; set; }
    }
}
