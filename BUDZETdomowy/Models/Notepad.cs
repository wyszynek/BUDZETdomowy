using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeBudget.Models
{
    public class Notepad
    {
        [Key]
        public int Id { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        [Column(TypeName = "nvarchar(35)")]
        public string Title { get; set; }

        [Column(TypeName = "nvarchar(1000)")]
        public string? Description { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
