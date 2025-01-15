using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using Web_Form.Data;
using Web_Form.Interfaces;
using Web_Form.Models;
using Web_Form.ViewModels;


namespace Web_Form.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SalesforceService _salesforceService;
        
        private readonly ApiTokenService _apiTokenService;
        private readonly DbContext _context;

        public UserController(UserManager<ApplicationUser> userManager,SalesforceService salesforceService,ApiTokenService apiTokenService,DbContext dbContext)
        {
            _userManager = userManager;
            _salesforceService = salesforceService;
           
            _apiTokenService = apiTokenService;
            _context = dbContext;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult>  CreateSalesforceAccount()
        {
            var Currentuser = await _userManager.GetUserAsync(User);


            var model = new SalesforceAccountViewModel
            {
                FullName = Currentuser.Name,
                Email = Currentuser.Email
            };
            return View(model);  // Send user data to the form
        }
        [HttpPost]
        
        public async Task<IActionResult> CreateSalesforceAccount(SalesforceAccountViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var token = await _salesforceService.GetAccessTokenAsync();  // Get OAuth token
                await _salesforceService.CreateAccountAndContactAsync(model, token);  // Send data to Salesforce
                TempData["Success"] = "Account successfully created in Salesforce!";
                return RedirectToAction("SaleforceSuccess");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to create Salesforce Account: " + ex.Message;
                return View(model);
            }
        }
        [HttpPost]
        public async Task<IActionResult> GenerateApiToken(string userId)
        {
             // Get the currently authenticated user

            if (userId == null)
            {
                return Unauthorized(); // If no user is authenticated, return Unauthorized
            }

            var token = _apiTokenService.GenerateToken(userId); // Generate a token for the user
            return Ok(new { apiToken = token }); // Return the generated token
        }
        public IActionResult GetSecureData( string apiToken)
        {
            if (string.IsNullOrEmpty(apiToken))
            {
                return Unauthorized("API Token is required.");
            }

            var userId = _apiTokenService.GetUserIdByToken(apiToken); // Get the user ID by the token

            if (userId == null)
            {
                return Unauthorized("Invalid or expired API token.");
            }

            // Proceed with the logic for the authenticated user
            return Ok(new { message = "This is secure data", userId = userId });
        }
        public async Task<IActionResult> GetAggregatedResults(string apiToken)
        {
            var Tokenuser = _context.ApiTokens
                         .Where(u => u.Token == apiToken)
                         .Select(u => u.UserId)
                         .FirstOrDefault();


            if (Tokenuser == null)
            {
                return Unauthorized("Invalid API token.");
            }

            var results = _context.TblForms
                .Where(t => t.UserId == Tokenuser)
                .Select(t => new
                {
                    TemplateTitle = t.Title,
                    Responses = t.SubmissionCount,
                    Questions = t.TblQuestions.Select(q => new
                    {
                        q.Question,
                        q.QuestionType,
                        
                    })
                }).ToList();

            return Ok(results);
        }
        public IActionResult CreateJiraTicketForm()
        {
            return View();
        }
        public async Task<IActionResult> CreateJiraTicket(string summary, string priority, string link, string reportedByEmail)
        {
            // Jira API URL for creating issues
            string jiraApiUrl = "https://mehedihassan8785.atlassian.net/rest/api/3/issue";

            // Jira email and API token for authentication
            string jiraEmail = "mehedihassan8785@gmail.com";
            string jiraApiToken = "ATATT3xFfGF00Qs30obqH-EhFKFCS5rURQI6AmT5ZGORXPrFiYHP2EtssU5UoQ-OgHuuJqnD4lZOPiKU6hwpjcPsO1xYFTVDPb5WIrapHmVxvx5tpvkw0HirSTRgNqUTDKX8FYXWFYScrobBdEbUZZcyQuN-PujSEMX_c5LkDHK2tlplImhzgBg=179CC364";
            // Constructing the payload
            var payload = new
            {
                fields = new
                {
                    project = new { key = "KAN" }, // Replace with your Jira project key
                    summary = summary,
                    description = new
                    {
                        type = "doc",
                        version = 1,
                        content = new[]
                        {
                    new
                    {
                        type = "paragraph",
                        content = new[]
                        {
                            
                            new { type = "text", text = $"Reported by: {reportedByEmail}\n" },
                            new { type = "text", text = $"Link: {link}\n" }
                        }
                    }
                }
                    },
                    issuetype = new { name = "Task" }, // Replace with the issue type you want to use
                    priority = new { name = priority },
                    // "High", "Medium", "Low"
                                                                      // Replace with the custom field ID for "Status" (if necessary)
                }
            };

            // Serialize the payload to JSON
            string jsonPayload = JsonConvert.SerializeObject(payload);

            // Prepare the HTTP request
            using (HttpClient client = new HttpClient())
            {
                // Set the authentication header
                var authenticationString = $"{jiraEmail}:{jiraApiToken}";
                var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(authenticationString));
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);

                // Set the content type to JSON
                var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

                // Make the HTTP POST request
                HttpResponseMessage response = await client.PostAsync(jiraApiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    // Parse and return the issue key
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseBody);

                    return RedirectToAction("JiraSuccess"); // The Jira issue key (e.g., "PROJECT-1")
                }
                else
                {
                    throw new Exception("Error creating ticket in Jira: " + response.Content.ReadAsStringAsync().Result);
                }
            }
        }
        public IActionResult SaleforceSuccess()
        {
            return View();
        }
        public IActionResult JiraSuccess()
        {
            return View();
        }
        public async Task<List<dynamic>> GetJiraIssuesByUser()
        {
            string jiraEmail = "mehedihassan8785@gmail.com";
            string jiraApiToken = "ATATT3xFfGF00Qs30obqH-EhFKFCS5rURQI6AmT5ZGORXPrFiYHP2EtssU5UoQ-OgHuuJqnD4lZOPiKU6hwpjcPsO1xYFTVDPb5WIrapHmVxvx5tpvkw0HirSTRgNqUTDKX8FYXWFYScrobBdEbUZZcyQuN-PujSEMX_c5LkDHK2tlplImhzgBg=179CC364";
            string jiraApiUrl = "https://mehedihassan8785.atlassian.net/rest/api/3/search";
            using (HttpClient client = new HttpClient())
            {
                var authenticationString = $"{jiraEmail}:{jiraApiToken}";
                var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(authenticationString));
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);

                // Add JQL to fetch issues reported by the current user
                var jql = "reporter = currentUser() ORDER BY created DESC";
                var queryParams = $"?jql={Uri.EscapeDataString(jql)}&fields=summary,status";  // Correctly escaped query string
                var response = await client.GetAsync(jiraApiUrl + queryParams);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseBody);

                    // Extract the issues from the response
                    var Issues = new List<dynamic>();

                    // Check if issues are available and loop through them
                    if (jsonResponse.issues != null)
                    {
                        foreach (var issue in jsonResponse.issues)
                        {
                            // Extract required fields (summary and status)
                            var issueData = new
                            {
                                Summary = issue.fields?.summary,
                                Status = issue.fields?.status?.name
                            };

                            Issues.Add(issueData);
                        }
                    }

                    return Issues;  // Returning the list of issues
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    throw new Exception("Error fetching Jira issues: " + errorResponse);
                }
            }
        }




    }
}
