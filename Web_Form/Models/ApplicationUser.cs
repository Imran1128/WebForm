using Microsoft.AspNetCore.Identity;

namespace Web_Form.Models
{
    public class ApplicationUser:IdentityUser
    {
        public string? Name { get; set; }
        public bool IsAdmin { get; set; }
        public bool Liked { get; set; }
        

    }
}
