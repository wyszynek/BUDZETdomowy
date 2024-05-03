using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HomeBudget.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

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
    }

    public class UserHelper
    {
        public static int GetCurrentUserId(HttpContext context)
        {
            var id = context.User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
            if (id is null)
            {
                throw new Exception("No current user");
            }

            return int.Parse(id);
        }

        public static string HashSHA256(string password)
        {
            StringBuilder sb = new StringBuilder();

            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(password));

                foreach (byte b in result)
                    sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }
    }

}
