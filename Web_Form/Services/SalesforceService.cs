using Newtonsoft.Json;
using System.Text;
using Web_Form.ViewModels;

public class SalesforceService
{
    private readonly HttpClient _client;

    public SalesforceService(HttpClient client)
    {
        _client = client;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        var response = await _client.PostAsync("https://login.salesforce.com/services/oauth2/token",
            new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("client_id", "3MVG9GCMQoQ6rpzSOOGbXwF05ZoboJXRuuUC8tRI3BFi0wTpCXKRy5USWuDHW6qNE3ctbctZsUA.iaJY3djY8"),
            new KeyValuePair<string, string>("client_secret", "644EAD4CAB6F11E9D51AA628AEDC02ADFEFD3171CECEBC7E24FF8CD6A31E618A"),
            new KeyValuePair<string, string>("username", "mehedihassan8785@gmail.com"),
            new KeyValuePair<string, string>("password", "Imran806926@")
            }));

        var content = await response.Content.ReadAsStringAsync();

        // Log the response content for debugging
        Console.WriteLine("Salesforce OAuth Response: " + content);

        if (!response.IsSuccessStatusCode)
        {
            // Log any errors in the response
            throw new Exception($"Error retrieving access token: {content}");
        }

        try
        {
            // Try deserializing the response into the expected structure
            var token = JsonConvert.DeserializeObject<dynamic>(content).access_token;
            return token;
        }
        catch (JsonReaderException jsonEx)
        {
            // Handle JSON deserialization errors
            throw new Exception("Failed to parse Salesforce OAuth response: " + jsonEx.Message);
        }
    }


    public async Task CreateAccountAndContactAsync(SalesforceAccountViewModel model, string token)
    {
        // Configure the client with authorization
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Prepare payload for Account creation
        var accountPayload = new
        {
            Name = model.FullName
        };

        // Send Account creation request
        var accountResponse = await _client.PostAsync(
            "https://itransition-9b-dev-ed.develop.my.salesforce.com/services/data/v52.0/sobjects/Account",
            new StringContent(JsonConvert.SerializeObject(accountPayload), Encoding.UTF8, "application/json")
        );

        var accountContent = await accountResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"Account Response: {accountContent}");

        if (!accountResponse.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to create Account: {accountContent}");
        }

        dynamic accountResult = JsonConvert.DeserializeObject(accountContent);
        string accountId = accountResult.id; // Extract Account ID

        if (string.IsNullOrEmpty(accountId))
        {
            throw new Exception("Account creation response is missing an ID.");
        }

        // Prepare payload for Contact creation
        var contactPayload = new
        {
            FirstName = model.FullName?.Split(' ').FirstOrDefault(),
            LastName = model.FullName?.Split(' ').Skip(1).FirstOrDefault() ?? model.FullName,
            Email = model.Email,
            Phone = model.Phone,
            AccountId = accountId
        };

        // Send Contact creation request
        var contactResponse = await _client.PostAsync(
            "https://itransition-9b-dev-ed.develop.my.salesforce.com/services/data/v52.0/sobjects/Contact",
            new StringContent(JsonConvert.SerializeObject(contactPayload), Encoding.UTF8, "application/json")
        );

        var contactContent = await contactResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"Contact Response: {contactContent}");

        if (!contactResponse.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to create Contact: {contactContent}");
        }

        dynamic contactResult = JsonConvert.DeserializeObject(contactContent);
        Console.WriteLine($"Contact successfully created with ID: {contactResult.id}");
    }


}
