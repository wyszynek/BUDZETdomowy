using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeBudget.Models
{
    public class Receipt2
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "nvarchar(75)")]
        public string Name { get; set; }

        [Column(TypeName = "nvarchar(150)")]
        public string Description { get; set; }

        public string ContentType { get; set; }

        public byte[]? Data { get; set; }

        public Receipt2()
        {
            ContentType = "JPG";
        }

        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
