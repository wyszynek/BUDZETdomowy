using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeBudget.Models
{
    public class Event
    {
        public int Id { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Title should be between 2 and 30 characters")]
        public string Title { get; set; }

        [Required]
        public DateTime Start { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [CheckDate(ErrorMessage = "You cannot enter the date in the past!")]
        public DateTime End { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string IsCompleted { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
