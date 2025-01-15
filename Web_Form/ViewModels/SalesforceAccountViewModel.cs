using System.ComponentModel.DataAnnotations;

namespace Web_Form.ViewModels
{
    public class SalesforceAccountViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }

        [Display(Name = "Company Name")]
        public string? CompanyName { get; set; } // Optional, depending on your use case.

        [Display(Name = "Job Title")]
        public string? JobTitle { get; set; } // Optional, depending on your use case.
    }
}
