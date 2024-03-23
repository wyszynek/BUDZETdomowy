using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace BUDZETdomowy.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Please enter the user name")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name should be between 2 and 30 characters")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Please enter the name")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name should be between 2 and 15 characters")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Please enter the first name")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name should be between 2 and 30 characters")]
        public string LastName { get; set; }

        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        public void HashPasswordSHA256()
        {
            StringBuilder Sb = new StringBuilder();

            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(Password));

                foreach (byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            Password = Sb.ToString();
        }

        public IEnumerable<Account> Accounts { get; set; } = new List<Account>();
    }
}
