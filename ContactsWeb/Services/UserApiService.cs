using ContactsWeb.Models.External;
using System.Text.Json;
using System.Web;

namespace ContactsWeb.Services;

public class UserApiService : IUserApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserApiService> _logger;
    private const string BaseUrl = "https://jsonplaceholder.typicode.com/users";

    public UserApiService(HttpClient httpClient, ILogger<UserApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<User?> FetchUserByEmailAsync(string email, CancellationToken token)
    {
        try
        {
            var encodedEmail = HttpUtility.UrlEncode(email);
            var url = $"{BaseUrl}?email={encodedEmail}";

            _logger.LogInformation("Fetching user data from API for e-mail: {Email}", email);
            var response = await _httpClient.GetAsync(url, token);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("API request failed with status {StatusCode} for e-mail {Email}", response.StatusCode, email);
                return null;
            }

            var jsonContent = await response.Content.ReadAsStringAsync(token);

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
            };

            var users = JsonSerializer.Deserialize<User[]>(jsonContent, jsonOptions);
            var user = users?.FirstOrDefault();

            if (user is not null)
            {
                _logger.LogInformation("User data found in API for e-mail {Email}: {UserName}", email, user.Name);
            }
            else
            {
                _logger.LogInformation("User not found in API for {email}.", email);
            }
            return user;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing JSON response for e-mail {Email}", email);
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching user data for e-mail {Email}", email);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when fetching user data for e-mail {Email}", email);
            return null;
        }

    }
}
