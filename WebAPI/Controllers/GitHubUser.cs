using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebAPI.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("[controller]")]
    public class GitHubUser2FreshDeskController : ControllerBase
    {
        string? GitHub_Token = Environment.GetEnvironmentVariable("GITHUB__TOKEN");
        string? FreshDesk_Token = Environment.GetEnvironmentVariable("FRESHDESK__TOKEN");

        private readonly ILogger<GitHubUser2FreshDeskController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        //private HttpClient _httpClient;

        public GitHubUser2FreshDeskController(ILogger<GitHubUser2FreshDeskController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            if (string.IsNullOrEmpty(GitHub_Token))
            {
                LogAndThrowException("GITHUB__TOKEN environment variable is not set.");
            }
            if (string.IsNullOrEmpty(FreshDesk_Token))
            {
                LogAndThrowException("FRESHDESK__TOKEN environment variable is not set.");
            }
        }

        [HttpGet("{gitHubUsername}/{freshDeskSubdomain}", Name = "GitHubUser2FreshDesk")]
        public async Task<String> GitHubUser2FreshDesk(string gitHubUsername, string freshDeskSubdomain)
        {
            GitHubUser user = await GetGitHubUser(gitHubUsername);
            string returnValue = await setFreshDeskUser(freshDeskSubdomain, user);
            return returnValue;
        }
        //[HttpGet("gh/{username}", Name = "GetGitHubUser")]
        internal async Task<GitHubUser> GetGitHubUser(string username)
        {

            if (string.IsNullOrEmpty(username))
            {
                LogAndThrowException("Username can't be empty");
            }
            string responseBody = "";
            using (var _httpClient = _httpClientFactory.CreateClient())
            {
                _httpClient.BaseAddress = new Uri("https://api.github.com/");
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GitHub_Token);
                //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", GitHub_Token);
                _httpClient.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "Task/1.0");

                string endpoint = $"users/{username}";
                HttpResponseMessage response = await _httpClient.GetAsync(endpoint);
                responseBody = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {

                    try
                    {
                        return JsonSerializer.Deserialize<GitHubUser>(responseBody)!;
                    }
                    catch (JsonException ex)
                    {
                        LogAndThrowException($"Error deserializing user data for '{username}': {ex.Message}");
                    }
                }
            }
            LogAndThrowException($"Error getting user '{username}'. Returning Empty user structure with id: -1 -> {responseBody}");
            return GitHubUser.GitHubNullUser();
        }

        internal async Task<String> setFreshDeskUser(string freshDeskSubdomain, GitHubUser gitHubUser)
        {
            String returnValue = "OK";

            if (string.IsNullOrEmpty(freshDeskSubdomain))
            {
                LogAndThrowException("FeshDesk subdomain can't be empty");
            }

            using (var _httpClient = _httpClientFactory.CreateClient())
            {
                _httpClient.BaseAddress = new Uri($"https://{freshDeskSubdomain}.freshdesk.com/api/v2/");
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{FreshDesk_Token}:X")));
                var options = new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    WriteIndented = true,
                };
                string content = JsonSerializer.Serialize(new FreshDeskContact(gitHubUser), options);
                _logger.LogDebug(content);
                Console.Write(content);
                var response = await _httpClient.PostAsync("contacts", new StringContent(content, Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug("Contact created successfully.");
                    returnValue = "OK_New";

                }
                else
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug($"Failed to create contact. Response: {responseContent}");

                    using (JsonDocument document = JsonDocument.Parse(responseContent))
                    {
                        JsonElement root = document.RootElement;
                        if (root.TryGetProperty("errors", out var errorsArray) && errorsArray.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var errorElement in errorsArray.EnumerateArray())
                            {
                                // Check if "duplicate_value" is present
                                if (errorElement.TryGetProperty("code", out var code) && code.GetString() == "duplicate_value")
                                {
                                    // Get "user_id" from "additional_info"
                                    if (errorElement.TryGetProperty("additional_info", out var additionalInfo) && additionalInfo.TryGetProperty("user_id", out var userId))
                                    {
                                        _logger.LogDebug(userId.ToString());
                                        Console.WriteLine("User already created. ID: " + userId);

                                        // The user already exists. Updating the existing user
                                        response = await _httpClient.PutAsync($"contacts/{userId}", new StringContent(content, Encoding.UTF8, "application/json"));
                                        if (response.IsSuccessStatusCode)
                                        {
                                            Console.WriteLine("Contact updated successfully.");
                                            returnValue = "OK_Update";
                                        }
                                        else
                                        {
                                            responseContent = await response.Content.ReadAsStringAsync();
                                            _logger.LogDebug($"Failed to update contact. Response: {responseContent}");
                                            Console.WriteLine($"Failed to update contact. Response: {responseContent}");
                                            Console.WriteLine(responseContent);
                                            returnValue = $"KO: {responseContent}";
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
            }

            return returnValue;
        }

        private void LogAndThrowException(string err)
        {
            _logger.LogError(err);
            throw new Exception(err);
        }

    }
}