using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BUDZETdomowy.Models
{
    public class Notepad
    {
        [Key]
        public int NoteID { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        [Column(TypeName = "nvarchar(35)")]
        public string Title { get; set; }

        [Column(TypeName = "nvarchar(1000)")]
        public string? Description { get; set; }
    }
}
