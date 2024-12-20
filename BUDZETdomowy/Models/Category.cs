﻿using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeBudget.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter the name of the category")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Category should be between 2 and 20 characters")]
        public string CategoryName { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string Icon { get; set; } = "";

        [Column(TypeName = "nvarchar(50)")]
        public string Type { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
