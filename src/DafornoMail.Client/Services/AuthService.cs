using System.Net.Http.Json;
using System.Security.Claims;
using Blazored.LocalStorage;
using DafornoMail.Client.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;

namespace DafornoMail.Client.Services;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginModel model);
    Task<AuthResponse> RegisterAsync(RegisterModel model);
    Task LogoutAsync();
    Task<CurrentUser> CurrentUserInfo();
}

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly ILocalStorageService _localStorage;
    private readonly NavigationManager _navigationManager;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        HttpClient httpClient,
        AuthenticationStateProvider authStateProvider,
        ILocalStorageService localStorage,
        NavigationManager navigationManager,
        ILogger<AuthService> logger)
    {
        _httpClient = httpClient;
        _authStateProvider = authStateProvider;
        _localStorage = localStorage;
        _navigationManager = navigationManager;
        _logger = logger;
    }

    public async Task<AuthResponse> LoginAsync(LoginModel model)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", model);
            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

            if (response.IsSuccessStatusCode && !string.IsNullOrEmpty(result?.Token))
            {
                await _localStorage.SetItemAsync("authToken", result.Token);
                ((ApiAuthenticationStateProvider)_authStateProvider).MarkUserAsAuthenticated(result.Token);
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", result.Token);
                
                return new AuthResponse { IsSuccess = true };
            }

            return new AuthResponse { IsSuccess = false, Error = result?.Error ?? "Login failed" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return new AuthResponse { IsSuccess = false, Error = ex.Message };
        }
    }

    public async Task<AuthResponse> RegisterAsync(RegisterModel model)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", model);
            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

            if (response.IsSuccessStatusCode)
            {
                return new AuthResponse { IsSuccess = true };
            }

            return new AuthResponse 
            { 
                IsSuccess = false, 
                Error = result?.Error ?? "Registration failed" 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return new AuthResponse { IsSuccess = false, Error = ex.Message };
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            await _localStorage.RemoveItemAsync("authToken");
            ((ApiAuthenticationStateProvider)_authStateProvider).MarkUserAsLoggedOut();
            _httpClient.DefaultRequestHeaders.Authorization = null;
            _navigationManager.NavigateTo("/login");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
        }
    }

    public async Task<CurrentUser> CurrentUserInfo()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<CurrentUser>("api/auth/currentuserinfo");
            return response ?? new CurrentUser();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user info");
            return new CurrentUser();
        }
    }
}

public class AuthResponse
{
    public bool IsSuccess { get; set; }
    public string? Token { get; set; }
    public string? Error { get; set; }
}

public class CurrentUser
{
    public string? Id { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Avatar { get; set; }
    public List<string> Roles { get; set; } = new();
    public Dictionary<string, string> Claims { get; set; } = new();
}

public class ApiAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;

    public ApiAuthenticationStateProvider(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var savedToken = await _localStorage.GetItemAsync<string>("authToken");

        if (string.IsNullOrWhiteSpace(savedToken))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", savedToken);

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(savedToken), "jwt")));
    }

    public void MarkUserAsAuthenticated(string token)
    {
        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt"));
        var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
        NotifyAuthenticationStateChanged(authState);
    }

    public void MarkUserAsLoggedOut()
    {
        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        var authState = Task.FromResult(new AuthenticationState(anonymousUser));
        NotifyAuthenticationStateChanged(authState);
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        if (keyValuePairs == null) return claims;

        keyValuePairs.TryGetValue(ClaimTypes.Role, out var roles);
        keyValuePairs.TryGetValue("unique_name", out var email);
        keyValuePairs.TryGetValue("nameid", out var name);
        keyValuePairs.TryGetValue("name", out var fullName);

        if (roles != null)
        {
            if (roles.ToString()!.Trim().StartsWith("["))
            {
                var parsedRoles = System.Text.Json.JsonSerializer.Deserialize<string[]>(roles.ToString()!);
                claims.AddRange(parsedRoles!.Select(role => new Claim(ClaimTypes.Role, role)));
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, roles.ToString()!));
            }
            keyValuePairs.Remove(ClaimTypes.Role);
        }

        if (email != null)
        {
            claims.Add(new Claim(ClaimTypes.Email, email.ToString()!));
            keyValuePairs.Remove("unique_name");
        }

        if (name != null)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, name.ToString()!));
            keyValuePairs.Remove("nameid");
        }

        if (fullName != null)
        {
            claims.Add(new Claim(ClaimTypes.Name, fullName.ToString()!));
            keyValuePairs.Remove("name");
        }


        claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value?.ToString() ?? string.Empty)));
        
        return claims;
    }


    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}
