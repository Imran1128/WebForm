using Google;
using Web_Form.Data;


public class ApiTokenService
{
    private readonly DbContext _context;

    public ApiTokenService(DbContext context)
    {
        _context = context;
    }

    public string GenerateToken(string userId)
    {
        var token = Guid.NewGuid().ToString(); // Generate a unique token (you can use any method you like)

        // Create a new ApiToken object and store it in the database
        var apiToken = new ApiToken
        {
            UserId = userId,  // Associate with the current user
            Token = token,    // The generated token value
            CreatedAt = DateTime.UtcNow // Timestamp
        };

        _context.ApiTokens.Add(apiToken); // Add the token record to the database
        _context.SaveChanges(); // Save changes to the database

        return token; // Return the generated token
    }

    public string GetTokenByUserId(string userId)
    {
        // Retrieve the API token for a specific user
        var token = _context.ApiTokens
                             .FirstOrDefault(t => t.UserId == userId); // Query by UserId
        return token?.Token; // Return the token (or null if not found)
    }
    public string GetUserIdByToken(string token)
    {
        var apiToken = _context.ApiTokens.FirstOrDefault(t => t.Token == token);
        return apiToken?.UserId; // Return the UserId if the token is found, otherwise null
    }

}
