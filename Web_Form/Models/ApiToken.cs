using Web_Form.Models;

public class ApiToken
{
    public int Id { get; set; }
    public string Token { get; set; } // The generated token
    public DateTime CreatedAt { get; set; } // Timestamp of when the token was created
    public string UserId { get; set; } // Foreign Key to User (e.g., ApplicationUser)
    public ApplicationUser User { get; set; } // Navigation Property to the User
}
