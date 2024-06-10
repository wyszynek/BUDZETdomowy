using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HomeBudget.Models.Enum;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HomeBudget.Models
{
    public class SourceOfIncome
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public ContractType ContractType { get; set; }

        public SalaryType SalaryType { get; set; }

        [Range(0.01, 100.00, ErrorMessage = "VAT should be greater than 0 and less than 100.")]
        [Column(TypeName = "decimal(18, 2)")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public decimal VAT { get; set; }

        public HealthInsuranceType HealthInsuranceType { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public decimal Ratio { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
